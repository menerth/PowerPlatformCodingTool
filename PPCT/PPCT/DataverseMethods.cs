using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using PPCT.Models.Dataverse;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace PPCT
{
    public static class DataverseMethods
    {
        public static async Task<Solution> GetSolutionInformation(ServiceClient serviceClient, string solutionName)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                throw new Exception("Solution name not configured!!!");
            }

            var solutionQuery = new QueryExpression(Solution.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(Solution.Fields.Id, Solution.Fields.UniqueName),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(Solution.Fields.UniqueName, ConditionOperator.Equal, solutionName)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity(Solution.EntityLogicalName, Publisher.EntityLogicalName, Solution.Fields.PublisherId, Publisher.Fields.Id, JoinOperator.Inner)
                    {
                        EntityAlias = "pub",
                        Columns = new ColumnSet(Publisher.Fields.CustomizationPrefix),
                    }
                }
            };

            var solutionResult = await serviceClient.RetrieveMultipleAsync(solutionQuery).ConfigureAwait(false);

            var solutionRecord = solutionResult.Entities.FirstOrDefault() ?? throw new Exception("Solution not found!!!");
            var solution = solutionRecord.ToEntity<Solution>();
            solution.publisher_solution = new Publisher()
            {
                CustomizationPrefix = solution.GetAttributeValue<AliasedValue>($"pub.{Publisher.Fields.CustomizationPrefix}").Value.ToString()
            };

            return solution;
        }
    }
}
