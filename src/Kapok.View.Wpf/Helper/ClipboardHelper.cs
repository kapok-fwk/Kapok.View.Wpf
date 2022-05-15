using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml;
using Kapok.Core;

namespace Kapok.View.Wpf;

public static class ClipboardHelper
{
    public static List<object[]> ParseClipboardData()
    {
        List<object[]> clipboardData = new List<object[]>();

        // get the data and set the parsing method based on the format
        // currently works with CSV and Text DataFormats            
        IDataObject? dataObj = Clipboard.GetDataObject();

        if (dataObj != null)
        {
            string[] formats = dataObj.GetFormats();

            const string dataFormatsXmlSpreadsheet = "XML Spreadsheet";
            if (formats.Contains(dataFormatsXmlSpreadsheet))
            {
                const string spreadsheetNamespaceUri = "urn:schemas-microsoft-com:office:spreadsheet";

                MemoryStream ms = dataObj.GetData(dataFormatsXmlSpreadsheet) as MemoryStream;
                XmlDocument xml = new XmlDocument();
                Debug.Assert(ms != null, nameof(ms) + " != null");
                xml.Load(ms);

                XmlNodeList table = xml.GetElementsByTagName("Table");

                var columnCountString = table[0].Attributes?.GetNamedItem("ExpandedColumnCount", spreadsheetNamespaceUri)?.InnerText;
                if (columnCountString == null)
                    throw new NotSupportedException($"Excel spreadsheet does not give information about the number of columns in tag Table, attribute ExpandedColumnCount in namespace {spreadsheetNamespaceUri}.");
                int columnCount = int.Parse(columnCountString, CultureInfo.InvariantCulture);

                foreach (XmlNode row in table[0].ChildNodes)
                {
                    if (row.Name.ToLower() == "row")
                    {
                        var rowIndexString = row.Attributes?.GetNamedItem("Index", spreadsheetNamespaceUri)?.InnerText;
                        if (rowIndexString != null)
                        {
                            int index = int.Parse(rowIndexString);

                            for (int n = clipboardData.Count; n < index - 1; n++)
                            {
                                clipboardData.Add(new object[columnCount]); // add empty rows
                            }
                        }

                        var lineCells = new object[columnCount];
                        int i = 0;
                            
                        foreach (XmlNode cell in row.ChildNodes)
                        {
                            var cellIndexString = cell.Attributes?.GetNamedItem("Index", spreadsheetNamespaceUri)?.InnerText;
                            if (cellIndexString != null)
                            {
                                int index = int.Parse(cellIndexString);

                                i = index - 1; // the 'ss:Index' attribute starts with 1, not with 0.
                            }

                            if (cell.ChildNodes.Count > 0)
                            {
                                var dataNode = cell.ChildNodes[0];

                                var typeString = dataNode.Attributes?.GetNamedItem("Type", spreadsheetNamespaceUri)
                                    ?.InnerText;
                                object cellValue;
                                switch (typeString)
                                {
                                    case "DateTime":
                                        cellValue = DateTime.Parse(cell.InnerText, CultureInfo.InvariantCulture);
                                        break;
                                    case "Number":
                                        cellValue = decimal.Parse(cell.InnerText, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                                        break;
                                    // ReSharper disable once RedundantCaseLabel
                                    case "String":
                                    default:
                                        cellValue = cell.InnerText;
                                        break;
                                }

                                lineCells[i++] = cellValue;
                            }
                            else
                            {
                                i++; // empty cell
                            }
                                
                        }
                        clipboardData.Add(lineCells);
                    }
                }

            }
            else if (formats.Contains(DataFormats.CommaSeparatedValue))
            {
                string clipboardString = (string)dataObj.GetData(DataFormats.CommaSeparatedValue);
                {
                    // EO: Subject to error when a CRLF is included as part of the data but it work for the moment and I will let it like it is
                    // WARNING ! Subject to errors
                    string[] lines = clipboardString?.Split(new[] { "\r\n" }, StringSplitOptions.None) ?? new string[] {};

                    foreach (string line in lines)
                    {
                        var lineValues = CsvHelper.ParseLineCommaSeparated(line);
                        if (lineValues != null)
                        {
                            clipboardData.Add(lineValues.Cast<object>().ToArray());
                        }
                    }
                }
            }
            else if (formats.Contains(DataFormats.Text))
            {
                string clipboardString = (string)dataObj.GetData(DataFormats.Text);
                clipboardData = CsvHelper.ParseText(clipboardString)
                    // cast List<string[]> to List<object[]>
                    .Select(r => r.Cast<object>().ToArray()).ToList();
            }
        }

        return clipboardData;
    }
}