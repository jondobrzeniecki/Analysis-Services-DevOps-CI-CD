# Analysis Services DevOps CI/CD

This repository is an up-to-date summarized example of creating a build and release pipeline (CI/CD) for Analysis Services Tabular models. The approach is centered around the .bim file and the use of <a href="https://github.com/otykier/TabularEditor">Tabular Editor</a> from the command line.  The example has a few specific steps for Azure Analysis Services, but the same approach can be used for a self-installed SQL Server hosting Analysis Services.

Prior working knowledge is assumed for:
* SQL Server Data Tools (SSDT) projects for Analysis Services Tabular models
* Using Git repositories
* Creating build and release pipelines in Azure DevOps.

## Prerequisites
* A running instance of <a href="https://azure.microsoft.com/en-us/services/analysis-services/">Azure Analysis Services</a>.
* An Analysis Services Tabular model project in <a href="https://docs.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt?view=sql-server-ver15">SQL Server Data Tools (SSDT)</a>.
* An <a href="https://dev.azure.com/">Azure DevOps</a> project with repository for SSDT project.
* Create a Service Principal (SPN), see <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#register-an-application-with-azure-ad-and-create-a-service-principal" target="_blank">here</a>.
* Add SPN as an Analysis Services Administrator using SQL Server Management Studio (SSMS), see <a href="https://docs.microsoft.com/en-us/azure/analysis-services/analysis-services-addservprinc-admins#using-sql-server-management-studio" target="_blank">here</a>.
  * Use <b>Manual Entry</b> with the following format, ```app:<ApplicationID>@<TenantID>```
  
## Repository Items
* <b>ReleasePipelineDataSourceUpdate.cs</b>: Script that is used during deployment to replace the server propetry value based on an release pipeline variable with the same name as the Data Source in the Tabular Model.
* <b>BuldPipeline.yml</b>: Yaml file for build pipeline that ensure the .bim file and the deployment scripts are available as artifacts for the relase pipeline.

## Deployment

<b>Step 1: Commit and Push to Source Control</b>
Commit and Push the Tabular model project to the Git repository of your Azure DevOps project. Ensure that the <b>ReleasePipelineDataSourceUpdate.cs</b> is also present in the repository.

<b>Step 2: Create build artifact containing Bim file</b>
Create a new build pipeline. Use the tasks from the steps section of the BuildPipeline.yml file. You should not have to modify either of the steps.

<b>Step 3: Create release pipeline</b>
Create a new release pipeline, adding the artifact from the build pipeline created in Step 2.

Add a Stage to the pipeline containing two steps:
1) PowerShell task with the following inline command to download and install Tabular Editor on the release agent.
```
# Download URL for Tabular Editor portable:
$TabularEditorUrl = "https://github.com/otykier/TabularEditor/releases/download/2.9.2/TabularEditor.Portable.zip" 

# Download destination (root of PowerShell script execution path):
$DownloadDestination = join-path (get-location) "TabularEditor.zip"

# Download from GitHub:
Invoke-WebRequest -Uri $TabularEditorUrl -OutFile $DownloadDestination

# Unzip Tabular Editor portable, and then delete the zip file:
Expand-Archive -Path $DownloadDestination -DestinationPath (get-location).Path
Remove-Item $DownloadDestination
```
2) Add a pipeline variable named ASConnectionString for the connection string to connect to the Azure Analysis Services server.  Using the following format for the connection string and lock the variable to hide the value since a set of SPN credentials are contained in it.

`Provider=MSOLAP;Data Source=<aas-server>;User ID=<app:cliend-id@tenant-id>;Password=<secret>`
 
3) Add a variable named ASModelName for the name of your tabular model.

4) Add variables for each data source in the model. The name of the variable should be the exact name of the data source in the tabular model.  The value will be the server name of the data source in the higher environment (i.e. Test, QA, Prod).

5) Add a command line task to the release agent after the PowerShell task. Use the script below to execute a deployment with Tabular Editor.

`start /B /wait TabularEditor.exe "$(System.DefaultWorkingDirectory)\_BimFileArtifact\theBimFile\s\<your-project-name>\<your-bim-file>.bim" -D "$(ASConnectionString)" "$(ASModelName)" -S "$(System.DefaultWorkingDirectory)\_BimFileArtifact\theBimFile\s\ReleasePipelineDataSourceUpdate.cs" -C -O -P -V -E -W

Your final pipeline stage should resemble the image below.




## References
* <a href="https://github.com/otykier/TabularEditor">Tabular Editor</a>
* <a href="https://github.com/otykier/TabularEditor/wiki/Command-line-Options">Tabular Editor Command line Options</a>
* <a href="https://tabulareditor.com/2019/10/08/DevOps3.html">You're Deploying it Wrong! - AS Edition</a>  

## License
<a href="https://github.com/jondobrzeniecki/Analysis-Services-DevOps-CI-CD/blob/master/LICENSE">MIT</a>

  

 
 

