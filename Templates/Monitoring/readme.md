# Azure Monitoring Templates

These are Azure Templates (ARM) that are used to create the respective resources in the Azure Portal.

Run these only after you have your Application Insights created.

Plugin queries (even in the dashboard) are now created to monitor custom plugins. For now, the common beginning of plugin namespace is to be used in the namespace parameter(s).

## Query Pack

This template(s) create query pack(s) you can run against your App Insights.

The query pack(s) are currently consisting some basic queries, as your monitoring needs might differ.

Adding your own or editing existing queries can be then done in Azure Portal.

## Dashboard

This template(s) creates dashboards in Azure Portal to provide quick overview.

Unfortunately dashboard(s)' underlying queries cannot be tied to queries in query packs, so you have to manually update queries in the dashboard if needed.

In case you would like to have something tied to the query pack, you would have to opt for Workbooks.

[Azure Monitoring](https://learn.microsoft.com/en-us/azure/azure-monitor/best-practices-analysis)

### Deployment

#### Fully manual

1. Download respective template(s)
2. Go to [Azure Portal](https://portal.azure.com/#create/Microsoft.Template)
3. Navigate to **Build your own template in editor**
4. Upload the template & populate parameters
5. Review & deploy

#### Remote file deployment

1. Use the following links to load deployment template from this repository
2. Populate parameters
3. Review & deploy

|Resource|Version|Link|Documentation|
|-|-|-|-|
|![](https://cloud-icons.onemodel.app/azure/other/01085-icon-service-Log-Analytics-Query-Pack.svg) Query Pack|0.1|[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmenerth%2FPowerPlatformCodingTool%2FFEAT-AddModularity%2FTemplates%2FMonitoring%2FPlugins%2FQueryPackTemplate%2Ftemplate.json)|[link](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/query-packs)|
|![](https://cloud-icons.onemodel.app/azure/general/10015-icon-service-Dashboard.svg) Dashboard|0.1|[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmenerth%2FPowerPlatformCodingTool%2FFEAT-AddModularity%2FTemplates%2FMonitoring%2FPlugins%2FDashboardTemplate%2Ftemplate.json)|[link](https://learn.microsoft.com/en-us/azure/azure-portal/azure-portal-dashboards)|

