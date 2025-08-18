async function getPullRequestsFromAzure(tfsNumber: string, organization: string, project: string, repository: string, personalAccessToken: string ): Promise<any> 
{
    const url = `https://dev.azure.com/${organization}/${project}/_apis/git/repositories/${repository}/pullrequests?searchCriteria.targetRefName=refs/tfs/${tfsNumber}&api-version=7.1-preview.1`;

    const headers = {
        'Authorization': `Basic ${btoa(`:${personalAccessToken}`)}`,
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    };

    try 
    {
        const response = await fetch(url, { headers });

        if (!response.ok) 
        {
            throw new Error(`Ошибка запроса. Код состояния: ${response.status}`);
        }

        const jsonData = await response.json();
        return jsonData;
    } 
    catch (error) 
    {
        console.error('Произошла ошибка:', error);
        throw error;
    }
}
