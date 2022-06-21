# Building Planning Poker
## Requirements
This application is based on Microsoft .NET Core LTS. You need to have the SDK installed to build the app.

## Classic .NET Build
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

## Docker
### Preparation
1. Install docker
	* [Installation instructions](https://docs.docker.com/engine/install/)
2. Clone the repo
	* `git clone https://github.com/kksoftwareag/partypoker.git`
3. Navigate into the solution directory
	* `cd partypoker`

### Option A: Dockerfile
4. Run docker build
	* `docker build .`
	output:
	```
	[...]
	=> => writing image sha256:bdf9fe339295350b11beadb3ee04e1ed92ea74c330c23b85c0a1f17537886471                       0.0s
	```
	copy the sha256 hash of the built image and then run the next command with that hash.

5. Run the image
	* `docker run bdf9fe339295350b11beadb3ee04e1ed92ea74c330c23b85c0a1f17537886471`

### Option B: docker-compose
4. Run docker-compose
	* `docker-compose up`
