// k6 baseline load test for OpenPsa.
//
// Run a seeded API on http://localhost:5000 (or override BASE_URL), then:
//   k6 run perf/baseline.js
//
// Knobs:
//   BASE_URL   default http://localhost:5000
//   USERNAME   default admin@openpsa.dev
//   PASSWORD   default admin
//   VUS        default 10        (steady-state virtual users)
//   DURATION   default 30s       (steady-state duration)
//
// Captures one-shot login latency, then in-loop list-endpoint latency for the
// hottest read paths. Thresholds are intentionally loose for a first baseline —
// tighten them once we have a few runs to compare against.

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const USERNAME = __ENV.USERNAME || 'admin@openpsa.dev';
const PASSWORD = __ENV.PASSWORD || 'admin';
const VUS      = parseInt(__ENV.VUS || '10', 10);
const DURATION = __ENV.DURATION || '30s';

const loginTrend = new Trend('login_duration', true);

export const options = {
    scenarios: {
        steady: {
            executor: 'constant-vus',
            vus: VUS,
            duration: DURATION,
            gracefulStop: '5s',
        },
    },
    thresholds: {
        http_req_failed:                       ['rate<0.01'],
        'http_req_duration{name:login}':       ['p(95)<400'],
        'http_req_duration{name:list_tickets}':['p(95)<300'],
        'http_req_duration{name:list_clients}':['p(95)<300'],
        'http_req_duration{name:dashboard}':   ['p(95)<500'],
    },
};

function login() {
    const res = http.post(
        `${BASE_URL}/api/auth/login`,
        JSON.stringify({ email: USERNAME, password: PASSWORD }),
        { headers: { 'Content-Type': 'application/json' }, tags: { name: 'login' } },
    );
    check(res, { 'login 200': (r) => r.status === 200 });
    loginTrend.add(res.timings.duration);
    const body = res.json();
    return body && (body.token || (body.data && body.data.token));
}

export function setup() {
    const token = login();
    if (!token) throw new Error('login failed — is the API up and seeded? POST /api/auth/login returned no token.');
    return { token };
}

export default function (data) {
    const headers = { Authorization: `Bearer ${data.token}` };

    group('reads', () => {
        const endpoints = [
            ['/api/tickets',   'list_tickets'],
            ['/api/clients',   'list_clients'],
            ['/api/contacts',  'list_contacts'],
            ['/api/projects',  'list_projects'],
            ['/api/dashboard', 'dashboard'],
        ];
        for (const [path, name] of endpoints) {
            const r = http.get(`${BASE_URL}${path}`, { headers, tags: { name } });
            check(r, { [`${name} ok`]: (x) => x.status === 200 || x.status === 204 });
        }
    });

    sleep(1);
}
