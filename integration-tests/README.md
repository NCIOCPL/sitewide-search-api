## Steps to Run Locally
1. Install docker (if not already installed)
2. Open a command prompt
3. Clone this repository
4. `cd <REPO_ROOT>/integration-tests`
5. `docker compose --project-directory docker-sws-api up --force-recreate`
6. Open a second command prompt
7. `cd <REPO_ROOT>/integration-tests`
8. `bin/load-integration-data.sh`
9. `bin/karate features`

## Notes
* There are [Docs](https://github.com/intuit/karate/blob/6de466bdcf105d72450a40cf31b8adb5c043037d/karate-netty/README.md#standalone-jar) for understanding how to run Karate standalone (including a description of the magic naming for the logging configuration).
* We use docker for dev testing because ES will no longer run on higher Java versions, so this is the easiest way to get it up and running.
* .NET running locally on a Mac cannot talk to ES because of how NEST always uses the host name to connect to ES and ES exposes the Virtual Machine's hostname/IP that runs Linux on the Mac.
* The `docker up` command above omits the "detached" (`-d`) option to make the containers easier to stop. It may be run either way.
* To re-run the data-loading script you must first execute
  * `curl -XDELETE "http://localhost:9200/autosg/?pretty"`
  * `curl -XDELETE "http://localhost:9200/cgov/?pretty"`
* In order to get the API rebuilt after changes to the code, you must run
  * `cd <REPO_ROOT>/integration-tests`
  * `docker compose --project-directory docker-sws-api down --rmi local`
  * `docker compose --project-directory docker-sws-api up --force-recreate`
  * Open a second command prompt
  * `cd <REPO_ROOT>/integration-tests`
  * `bin/load-integration-data.sh`
* The testing tool generates copious amounts of logging/reporting. For a pretty representation of the test results, open `<REPO_ROOT>/integration-tests/target/cucumber-html-reports/overview-features.html` in your favorite browser.
* If you run the integration tests anywhere other than the `integration-tests` directory, a `target` directory will be created with logging and other output.
