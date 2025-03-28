import { Writer } from 'k6/x/kafka';
import { randomIntBetween, randomString } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { encoding } from 'k6';

const brokers = ["kafka:9092"];
const topic = "user-ip-topic";
const testDuration = '30s';
const pauseDuration = '5s';
const targetUsers = 50000;

const writer = new Writer({
    brokers,
    topic,
    batchSize: 1000,  // Increase batch size for high throughput
});

function generateRandomIP() {
    return `${randomIntBetween(1, 255)}.${randomIntBetween(0, 255)}.${randomIntBetween(0, 255)}.${randomIntBetween(1, 254)}`;
}

function getRandomLong(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function generateRandomUserId() {
    return getRandomLong(1, 9000000000000000000);
}

export const options = {
    scenarios: {
        burst_load: {
            executor: 'ramping-arrival-rate',
            preAllocatedVUs: 1000,  // Warm VUs
            maxVUs: 5000,           // Maximum parallel VUs
            stages: [
                { target: targetUsers, duration: '5s' },  // Ramp-up
                { target: targetUsers, duration: testDuration },  // Sustain
                { target: 0, duration: '5s' },  // Ramp-down
            ],
            gracefulStop: pauseDuration,
        },
    },
    discardResponseBodies: true,
    noConnectionReuse: true,
};

export default function () {
    const payload = {
        userId: generateRandomUserId(),
        ip: generateRandomIP(),
        timestamp: new Date().toISOString(),
        payload: randomString(100),  // Additional random data
    };

    writer.produce({
        messages: [{
            key: encoding.encode(String(payload.userId)),
            value: JSON.stringify(payload),
        }],
    });
}

export function teardown() {
    writer.close();
}