using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetsWriter
{
    private const string formURL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSe-TFAR6xXF8vy5ApMmyXkGuMNbaxDJ3OXd-jkgeqHh8XlNkg/formResponse";

    public IEnumerator PostToForm(LoggingData data)
    {
        WWWForm form = new WWWForm();
        form.AddField(CreateEntryName("871638578"), data.playerGuid?.ToString() ?? "");
        form.AddField(CreateEntryName("913405570"), data.playerEvent.ToString());
        form.AddField(CreateEntryName("2060596638"), data.param1 ?? "");
        form.AddField(CreateEntryName("1035046308"), data.param2 ?? "");
        form.AddField(CreateEntryName("1540696516"), data.param3 ?? "");

        using (var uwr = UnityWebRequest.Post(formURL, form))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log("Successfully posted");
            }
        }
    }

    private string CreateEntryName(string entryId)
    {
        return $"entry.{entryId}";
    }
}

