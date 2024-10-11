using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingAdapter : MonoBehaviour
{
    private bool _isPostingOnline = true;
    private GoogleSheetsWriter _googleSheets;
    private CSVWriter _cvsWriter;

    private const string FileName = "analystics";

    public void InitAdapter(bool isLoggingInEditor)
    {
#if UNITY_EDITOR
        _isPostingOnline = isLoggingInEditor;
#endif

        _googleSheets = new GoogleSheetsWriter();
        _cvsWriter = new CSVWriter(FileName);
    }

    public void PostLog(LoggingData data)
    {
        if (_isPostingOnline)
        {
            StartCoroutine(_googleSheets.PostToForm(data));
        }

        _cvsWriter.PostToForm(data);
    }
}
