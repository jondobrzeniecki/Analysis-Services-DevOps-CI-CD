# Analysis Services DevOps CI/CD

This repository is an up-to-date summarized example of creating a build and release pipeline (CI/CD) for Analysis Services Tabular models. The approach is centered around the .bim file and the use of <a href="https://github.com/otykier/TabularEditor">Tabular Editor</a> from the command line.  The example has a few specific steps for Azure Analysis Services, but the same approach can be used for a self-installed SQL Server hosting Analysis Services.

Prior working knowledge is assumed for:
* SQL Server Data Tools (SSDT) projects for Analysis Services Tabular models.
* Azure DevOps projects and using Git repositories.
* Creating build and release pipelines in Azure DevOps.

<b>Table of Contents</b>
  * [Prerequisites](#prerequisites)
  * [Repository Items](#repository-items)
  * [Deployment](#deployment)
  * [Handling Secret Values](#handling-secret-values)
  * [Limitations](#limitations)
  * [References](#references)
  * [License](#license)

## Prerequisites
* A running instance of <a href="https://azure.microsoft.com/en-us/services/analysis-services/">Azure Analysis Services</a>.
* An Analysis Services Tabular model project in <a href="https://docs.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt?view=sql-server-ver15">SQL Server Data Tools (SSDT)</a>.
* An <a href="https://dev.azure.com/">Azure DevOps</a> project with repository for SSDT project.
* Create a Service Principal (SPN), see <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#register-an-application-with-azure-ad-and-create-a-service-principal" target="_blank">here</a>.
* Add SPN as an Analysis Services Administrator using SQL Server Management Studio (SSMS), see <a href="https://docs.microsoft.com/en-us/azure/analysis-services/analysis-services-addservprinc-admins#using-sql-server-management-studio" target="_blank">here</a>.
  * Use <b>Manual Entry</b> with the following format, ```app:<ApplicationID>@<TenantID>```
  
## Repository Items
* <b>ReleasePipelineDataSourceUpdate.cs</b>: Script that is used during deployment to replace the server property value based on an release pipeline variable with the same name as the Data Source in the Tabular Model.
* <b>BuldPipeline.yml</b>: Yaml file for build pipeline that ensure the .bim file and the deployment scripts are available as artifacts for the release pipeline.

## Deployment

<b>Step 1: Commit and Push to Source Control</b>
<br/>
Commit and Push the Tabular model project to the Git repository of your Azure DevOps project. Ensure that the <b>ReleasePipelineDataSourceUpdate.cs</b> script is also present in the repository.
<br/><br/><br/>
<b>Step 2: Create build artifact containing Bim file</b>
<br/>
Create a new build pipeline. Use the tasks from the steps section of the <b>BuildPipeline.yml</b> file. You should not have to modify either of the steps. The two steps copy the project files in the repository to a staging area on the build agent, and then publishes the project files as an artifact.  This positions the project files, including the Bim file, for consumption in the later release pipeline.  It also ensures that the Bim file is always present for the release pipeline.
<br/><br/><br/>
<b>Step 3: Create release pipeline</b>
<br/>
Create a new release pipeline, adding the artifact from the build pipeline created in Step 2. Add a Stage to the pipeline and follow the steps below to add tasks.
<br/>
<br/>
<b>1)</b> Add a PowerShell task with the following inline command to download and install Tabular Editor on the release agent.
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
<b>2)</b> Add a pipeline variable named ASConnectionString for the connection string to connect to the Azure Analysis Services server.  Using the following format for the connection string. Lock the variable to hide the value since a set of SPN credentials are contained in it.

`Provider=MSOLAP;Data Source=<aas-server>;User ID=app:<ApplicationID>@<TenantID>;Password=<Secret>`
 
<b>3)</b> Add a variable named ASModelName for the name of your tabular model.

<b>4)</b> Add variables for each data source in the model. The name of the variable should be the exact name of the data source in the tabular model.  The value will be the server name of the data source in the higher environment (i.e. Test, QA, Prod).

<b>5)</b> Add a command line task to the release agent after the PowerShell task. Use the script below to execute a deployment with Tabular Editor.

<i><p style="color:red">Note: Please reference the <a href="https://github.com/otykier/TabularEditor/wiki/Command-line-Options">Tabular Editor Command line Options</a> for a better understanding of the options present in the sample command.</p></i>

```
start /B /wait TabularEditor.exe "$(System.DefaultWorkingDirectory)\_BimFileArtifact\theBimFile\s\<your-project-name>\<your-bim-file>.bim" -D "$(ASConnectionString)" "$(ASModelName)" -S "$(System.DefaultWorkingDirectory)\_BimFileArtifact\theBimFile\s\ReleasePipelineDataSourceUpdate.cs" -C -O -P -V -E -W
```

If roles and members are mainted in the tabular model project and are deployed with the release pipeline, then the following script needs to be added to <b>ReleasePipelineDataSourceUpdate.cs</b> to remove an unsupported metadata tag related to role members.

```
foreach(var role in Model.Roles)
{
    // Find all Azure AD role members where MemberID is assigned:
    var orgMembers = role.Members.OfType<ExternalModelRoleMember>()
        .Where(m => m.IdentityProvider == "AzureAD" && !string.IsNullOrEmpty(m.MemberID)).ToList();
        
    // Delete the member and recreate it without assigning MemberID:
    foreach(var orgMember in orgMembers)
    {
        orgMember.Delete();
        role.AddExternalMember(orgMember.MemberName);
    }
}
```

<br/>
<br/>
Your final pipeline should resemble the images below:
<br/>
<br/>

![Release pipline](https://raw.githubusercontent.com/jondobrzeniecki/Analysis-Services-DevOps-CI-CD/main/img/ReleasePipeline.jpg)

![Release pipline agent tasks](https://raw.githubusercontent.com/jondobrzeniecki/Analysis-Services-DevOps-CI-CD/main/img/ReleasePipelineStage.jpg)

## Handling Secret Values
If you're updating the credentials used in a datasource (i.e. username, password) then follow the additional step below to make the secret values available to the <b>ReleasePipelineDataSourceUpdate.cs</b> script that will be excuted by the Tabular Editor command line task (Deployment - Step 5).

The command line task in the release pipeline has a section for <b>Environment Variables</b>. Use this section if you're hanlding secret values coming from either pipeline tasks or Azure Key Vault. In the screenshot below you can see an example of a local variable named <b>Password</b> being mapped to a pipeline variable that securely stores the password.
<br/>
<br/>
![Environment Variables settings](https://raw.githubusercontent.com/jondobrzeniecki/Analysis-Services-DevOps-CI-CD/main/img/SecretValuesCmdTask.png)

## Limitations
* This is a basic example that does not include other DevOps best practices (i.e. Unit testing, Quality control checks).
* The build pipeline copies all project files when only the Bim files is required. This was intentional for making the settings of the Copy Files task generic to increase reusability.

## References
* <a href="https://github.com/otykier/TabularEditor">Tabular Editor</a>
* <a href="https://github.com/otykier/TabularEditor/wiki/Command-line-Options">Tabular Editor Command line Options</a>
* <a href="https://tabulareditor.com/2019/10/08/DevOps3.html">You're Deploying it Wrong! - AS Edition</a>  

## License
<a href="https://github.com/jondobrzeniecki/Analysis-Services-DevOps-CI-CD/blob/master/LICENSE">MIT</a>

  

 
 

