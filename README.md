# Analysis Services DevOps CI/CD

This repository is an up-to-date summarized example of creating a build and release pipeline (CI/CD) for Analysis Services Tabular models. The approach is centered around the .bim file and the use of <a href="https://github.com/otykier/TabularEditor">Tabular Editor</a> from the command line.  The example has a few specific steps for Azure Analysis Services, but the same approach can be used for a self-installed SQL Server hosting Analysis Services.

Prior working knowledge of SQL Server Data Tools (SSDT) projects for Analysis Services Tabular models is assumed.

## Prerequisites
* A running instance of <a href="https://azure.microsoft.com/en-us/services/analysis-services/">Azure Analysis Services</a>.
* An Analysis Services Tabular model project in <a href="https://docs.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt?view=sql-server-ver15">SQL Server Data Tools (SSDT)</a>.
* An <a href="https://dev.azure.com/">Azure DevOps</a> project with repository for SSDT project.
* Create a Service Principal (SPN), see <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#register-an-application-with-azure-ad-and-create-a-service-principal" target="_blank">here</a>.
* Add SPN as an Analysis Services Administrator using SQL Server Management Studio (SSMS), see <a href="https://docs.microsoft.com/en-us/azure/analysis-services/analysis-services-addservprinc-admins#using-sql-server-management-studio" target="_blank">here</a>.
  * Use <b>Manual Entry</b> with the following format, ```app:<ApplicationID>@<TenantID>```
  
# Repository Items

# Deployment

# References
<a href="https://github.com/otykier/TabularEditor">Tabular Editor</a>
<a href="https://github.com/otykier/TabularEditor/wiki/Command-line-Options">Tabular Editor Command line Options</a>
<a href="https://tabulareditor.com/2019/10/08/DevOps3.html">You're Deploying it Wrong! - AS Edition</a>  

  

 
 

