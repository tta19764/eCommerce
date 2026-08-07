---
description: A document update workflow
---

Process the repository service by service.

For each service:

* inspect its layers;
* process Domain, Application, Infrastructure, API, and Tests where relevant;
* update XML documentation to match the current implementation;
* do not change runtime behavior;
* build or validate the service before moving on;
* continue until every service has been processed.

Do not stop after one service.
Do not perform unrelated refactoring.
Keep XML documentation concise and meaningful.
