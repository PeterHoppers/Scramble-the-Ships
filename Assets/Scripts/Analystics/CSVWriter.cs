using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVWriter
{
    private string _fileName = "";
    private string _filePath = "";
    private StringBuilder _csvStringBuilder = new StringBuilder();
    CultureInfo _culture = new CultureInfo("en-US");

    public CSVWriter(string fileName)
    {
        _fileName = fileName;
        _filePath = Path.Combine(Application.persistentDataPath, _fileName) + ".csv";
    }

    public void PostToForm(LoggingData logData)
    {
        var newLine = $"{DateTime.Now.ToString(_culture)},{logData.playerGuid},{logData.playerEvent},{logData.param1},{logData.param2},{logData.param3}";
        _csvStringBuilder.AppendLine(newLine);
        File.AppendAllText(_filePath, _csvStringBuilder.ToString());
    }
}
