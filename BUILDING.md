# Building Planning Poker
## Requirements
This application is based on Microsoft .NET Core LTS. You need to have the SDK installed to build the app.

## Steps to Build the Software
### Preparation
1. Install the version of .NET Core SDK as documented the project file in the XML tag `TargetFramework`. Currently it's .NET 6.
	* Installation instructions
		* [Windows](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60)
		* [macOS](https://docs.microsoft.com/en-us/dotnet/core/install/macos)
		* [Linux](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
2. Clone the repo
	* `git clone https://github.com/kksoftwareag/partypoker.git`
3. Navigate into the project directory
	* `cd partypoker`
	* `cd PlanningPoker.Web`

### Option A: Publish, then run the binary
4. Run the publish command
	* `dotnet publish -o=publish`
5. Navigate into the output folder
	* `cd publish`
6. Run the App
	* `dotnet PlanningPoker.Web.dll`
7. Navigate with a Webbrowser to the url: [localhost:5000](http://localhost:5000)

### Option B: Run
4. Run the app directly
	* `dotnet run`
5. Navigate with a Webbrowser to the url: [localhost:5000](http://localhost:5000)
