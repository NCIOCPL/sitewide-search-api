## Steps to Run Locally
1. Install docker (if not already installed)
1. Have NuGet configured correctly (See the "NuGet Configuration" topic below).
1. Ensure you have a Java JDK.
1. Open a command prompt
1. Clone this repository
1. `cd <REPO_ROOT>/integration-tests`
1. `docker compose --project-directory docker-sws-api up --force-recreate`
1. Open a second command prompt
1. `cd <REPO_ROOT>/integration-tests`
1. `bin/load-integration-data.sh`
1. `bin/karate features`

## NuGet Configuraiton
1. Create a GitHub [Personal Access token](https://github.com/settings/tokens/) with a descriptive name such as "NuGet package".
1. Assign the token the `packages:read` scope and save it.
1. Copy the token's value.
1. If you have the dotnet command line tool installed, run this command (it's all one line), substituting your username and token value.
    ```bash
    dotnet nuget add source https://nuget.pkg.github.com/nciocpl/index.json --name github --username <YOUR_GITHUB_USERNAME> --password <THE_TOKEN_VALUE> --store-password-in-clear-text
    ```
1. If you do NOT have the dotnet command line tool installed:
    2. Create `~/.nuget/NuGet/NuGet.Config`.
    2. Put these lines in the file (be sure to substitute your username and toke value)
       ```xml
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
            <packageSources>
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
                <add key="github" value="https://nuget.pkg.github.com/nciocpl/index.json" />
            </packageSources>
            <packageSourceCredentials>
                <github>
                    <add key="Username" value="<YOUR_GITHUB_USERNAME>" />
                    <add key="ClearTextPassword" value="<THE_TOKEN_VALUE>" />
                </github>
            </packageSourceCredentials>
            <disabledPackageSources>
            </disabledPackageSources>
        </configuration>
       ```

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
* For any features comparing against standard JSON responses you can use karate to write out those responses to `.json` files
  * `karate.write(response, 'filename')` will output the request response to a file in the `target` folder
  * If a test is failing before the `karate.write`, it will not execute. Comment out the failing line.
  * This is a shortcut to using Postman, curl, or Swagger to save out the results one by one
  * ***Keep in mind that doing this you are essentially testing 1 = 1 the first time you generate the result files.*** So be sure that you are confident in the results when you prep the files. Future tests against the files are then comparing back to the point in time that you created them.