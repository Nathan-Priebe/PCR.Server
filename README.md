# PCR.Server
The server component of the PCR project. Executes all required remote operations on the host windows PC

## Getting Started
Download the PCR.Server project files and also a copy of the PCR.Common project files, ensure both projects are in the same directory. Open the visual studio project for PCR.Server and ensure all nuget packages are installed and verfied, and the project was able to find the PCR.Common project. Rebuild the project and then rebuild the setup project to build a setup.msi/setup.exe

## Deployment
Run the Setup.msi on the host machine to install the server component. The application should automatically create the required inbound firewall rules, if it does not allow port 4442 through the firewall. Once the application has installed run the application and it should appear in your host machines tray.

## Built With
* WPF
* OWIN
* Web API
