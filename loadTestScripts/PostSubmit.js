// import http from 'k6/http';
// import { check } from 'k6';
// import { Rate } from 'k6/metrics';

// export const errorRate = new Rate('errors');

// export default function () {
//   const url = `https://localhost:5013/WeatherForecast`;
  
//   const payload = JSON.stringify({"userID":6333});

//   check(http.post(url, payload), {
//     'status is 200': (r) => r.status == 200,
//   }) || errorRate.add(1);
// }

import http from 'k6/http';
import { check } from 'k6';
import { Rate } from 'k6/metrics';

export const errorRate = new Rate('errors');

export default function () {
  const url = `${__ENV.MY_HOSTNAME}/WeatherForecast`;
  const params = {
    headers: {
      'Authorization': `Bearer ${__ENV.TOKEN}`,
      'Content-Type': 'application/json',
    },
  };
  
  const payload = JSON.stringify({"userID":6333});
  check(http.post(url, payload,params), {
    'status is 200': (r) => r.status == 200,
  }) || errorRate.add(1);
}