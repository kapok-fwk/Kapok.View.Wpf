using Kapok.Data;
using Kapok.Report;
using Kapok.Report.DataModel;
using System.Collections.ObjectModel;
using System.IO;
using DataTableReportViewModelRes = Kapok.View.Wpf.Report.Resources.DataTableReportViewModel;
using Res = Kapok.View.Wpf.Report.Resources.MimeTypeReportPage;

namespace Kapok.View.Wpf.Report;

public sealed class MimeTypeReportPage : ReportPage<ReportProcessor<Kapok.Report.Model.Report>, Kapok.Report.Model.Report>
{
    private MimeTypeViewModel? _selectedMimeType;
    private readonly ReportEngine _reportEngine;

    private const string MimeTypeExcel2007 = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string MimeTypeExcel2003 = "application/vnd.ms-excel";
    private const string MimeTypeCsv = "text/csv";

    static MimeTypeReportPage()
    {
        WpfViewDomain.RegisterPageWpfWindowConstructor<MimeTypeReportPage>(() => new MimeTypeReportPageWindow());

        // TODO: move this out of this assembly
        // ReSharper disable StringLiteralTypo
        MimeTypeDisplayName = new Dictionary<string, Caption>
        {
            {
                "image/bmp",
                new Caption
                {
                    {"en-US", "Bitmap file (*.bmp)"},
                    {"de-DE", "Bitmap Datei (*.bmp)" }
                }
            },
            {
                    
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                new Caption
                {
                    {"en-US", "Word file 2007+ (*.docx)"},
                    {"de-DE", "Word Datei 2007+ (*.docx)" }
                }
            },
            {
                "text/html",
                new Caption
                {
                    {"en-US", "HTML file (*.html)"},
                    {"de-DE", "HTML Datei (*.html)" }
                }
            },
            {
                "image/jpeg",
                new Caption
                {
                    {"en-US", "JPEG image file (*.jpg)"},
                    {"de-DE", "JPEG Bild Datei (*.jpg)" }
                }
            },
            {
                "application/json",
                new Caption
                {
                    {"en-US", "JSON file (*.json)"},
                    {"de-DE", "JSON Datei (*.json)" }
                }
            },
            {
                "image/emf",
                new Caption
                {
                    {"en-US", "Windows Meta file (*.emf)"},
                    {"de-DE", "Windows Meta Datei (*.emf)" }
                }
            },
            {
                "message/rfc822",
                new Caption
                {
                    {"en-US", "MHtml file (*.mhtml)"},
                    {"de-DE", "MHtml Datei (*.mhtml)" }
                }
            },
            {
                "application/pdf",
                new Caption
                {
                    {"en-US", "PDF file (*.pdf)"},
                    {"de-DE", "PDF Datei (*.pdf)"}
                }
            },
            {
                "image/png",
                new Caption
                {
                    {"en-US", "PNG image file (*.png)"},
                    {"de-DE", "PNG Bild Datei (*.png)" }
                }
            },
            {
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                new Caption
                {
                    {"en-US", "PowerPoint file 2007+ (*.pptx)"},
                    {"de-DE", "PowerPoint Datei 2007+ (*.pptx)" }
                }
            },
            {
                "application/rtf",
                new Caption
                {
                    {"en-US", "Rich text format file (*.rtf)"},
                    {"de-DE", "Rich Text Format Datei (*.rtf)" }
                }
            },
            {
                "image/svg+xml",
                new Caption
                {
                    {"en-US", "SVG image file (*.svg)"},
                    {"de-DE", "SVG Bild file (*.svg)" }
                }
            },
            {
                "text/plain",
                new Caption
                {
                    {"en-US", "Text file (*.txt)"},
                    {"de-DE", "Text Datei (*.txt)" }
                }
            },
            {
                "image/tiff",
                new Caption
                {
                    {"en-US", "TIFF image file (*.tiff)"},
                    {"de-DE", "TIFF Bild Datei (*.tiff)" }
                }
            },
            {
                "application/xhtml+xml",
                new Caption
                {
                    {"en-US", "XHtml file (*.xhtml)"},
                    {"de-DE", "XHtml Datei (*.xhtml)" }
                }
            },
            {
                MimeTypeExcel2003,
                new Caption
                {
                    {"en-US", "Microsoft Excel 97-2003 file (*.xls)"},
                    {"de-DE", "Microsoft Excel 97-2003 Datei (*.xls)" }
                }
            },
            {
                MimeTypeExcel2007,
                new Caption
                {
                    {"en-US", "Microsoft Excel 2007+ (*.xlsx)"},
                    {"de-DE", "Microsoft Excel 2007+ (*.xlsx)"}
                }
            },
            {
                "application/xml",
                new Caption
                {
                    {"en-US", "XML file (*.xml)"},
                    {"de-DE", "XML Datei (*.xml)" }
                }
            },
            {
                "application/vnd.ms-xpsdocument",
                new Caption
                {
                    {"en-US", "XPS file (*.xps)"},
                    {"de-DE", "XPS Datei (*.xps)" }
                }
            },
            {
                MimeTypeCsv,
                new Caption
                {
                    {"en-US", "CSV file (*.csv)"},
                    {"de-DE", "CSV Datei (*.csv)" }
                }
            }
        };
        // ReSharper restore StringLiteralTypo
    }

    private static readonly IReadOnlyDictionary<string, Caption> MimeTypeDisplayName;

    public MimeTypeReportPage(Kapok.Report.Model.Report model, IDataDomain dataDomain, ReportLayout? layout = null, IViewDomain? viewDomain = null)
        : base(model, null, viewDomain)
    {
        _reportEngine = new ReportEngine(dataDomain);
        ReportLayout = _reportEngine.GetOrCreateReportLayout(model, null);

        IsDesignable = _reportEngine.IsModelDesignable(model);
        var supportedMimeTypes = _reportEngine.GetSupportedMimeTypes(model, layout);

        // UI
        SaveAsFileAction = new UIAction("SaveAsFile", Save, CanSave);
        DesignAction = new UIAction("Design", Design, CanDesign);

        SupportedMimeTypes = new ObservableCollection<MimeTypeViewModel>();

        SupportedMimeTypes.AddRange(
            from m in (
                from mimeType in supportedMimeTypes
                join d in MimeTypeDisplayName on mimeType equals d.Key into displayNameMap
                where MimeTypes.MimeTypeMap.GetExtension(mimeType, false) != string.Empty // NOTE: We hide here all mime types where we don't know the file extension for
                select new MimeTypeViewModel
                {
                    MimeType = mimeType,
                    FileExtension = MimeTypes.MimeTypeMap.GetExtension(mimeType, false),
                    DisplayName = displayNameMap.DefaultIfEmpty().FirstOrDefault().Value
                        ?.LanguageOrDefault(ViewDomain.Culture) ?? mimeType
                })
            orderby m.DisplayName
            select m
        );

        if (SupportedMimeTypes.Count == 1)
            SelectedMimeType = SupportedMimeTypes.First();
        else
        {
            var defaultMimeTypesOrder = new[]
            {
                MimeTypeExcel2007,
                MimeTypeExcel2003,
                MimeTypeCsv
            };

            foreach (var defaultMimeType in defaultMimeTypesOrder)
            {
                var mimeType = SupportedMimeTypes.FirstOrDefault(m => m.MimeType == defaultMimeType);
                if (mimeType != null)
                {
                    SelectedMimeType = mimeType;
                    break;
                }
            }
        }
    }

    public ReportLayout ReportLayout { get; set; }

    #region UI properties

    // ReSharper disable CollectionNeverQueried.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public bool ShowReportParameters => ReportParameters.Count > 0;

    public bool IsDesignable { get; }

    public MimeTypeViewModel? SelectedMimeType
    {
        get => _selectedMimeType;
        set => SetProperty(ref _selectedMimeType, value);
    }

    public ObservableCollection<MimeTypeViewModel> SupportedMimeTypes { get; }
        
    public IAction SaveAsFileAction { get; }

    public IAction DesignAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore CollectionNeverQueried.Global

    #endregion

    public class MimeTypeViewModel
    {
        public string? MimeType { get; set; }

        public string? FileExtension { get; set; }

        public string? DisplayName { get; set; }
    }

    private void Save()
    {
        string fileName = ViewDomain.OpenSaveFileDialog(Res.SaveAsFileDialogTitle, $"{SelectedMimeType.DisplayName} (*{SelectedMimeType.FileExtension})|*{SelectedMimeType.FileExtension}");
        if (fileName == null)
        {
            return;
        }

        try
        {
            using (var sw = new StreamWriter(fileName))
            {
                _reportEngine.ExecuteReport(
                    ReportModel,
                    ReportParameters.ToDictionary(p => p.ReportParameter.Name, p => p.Value),
                    SelectedMimeType.MimeType,
                    sw.BaseStream,
                    ReportLayout);
            }

            // TODO: Implement a nice Request page here showing the possible options as buttons with "Ignore/do this next time automatically" to be saved in the user setup
            if (ViewDomain.ShowConfirmMessage(
                    DataTableReportViewModelRes.SendNotificationOnExportSuccessfulRequestToOpen_Message,
                    DataTableReportViewModelRes.SendNotificationOnExportSuccessfulRequestToOpen_Caption,
                    this))
            {
                ViewDomain.OpenFile(fileName);
            }

            /*
            ViewDomain.ShowInfoMessage(
                DataTableReportViewModelRes.SendNotificationOnExportSuccessful_Message,
                DataTableReportViewModelRes.SendNotificationOnExportSuccessful_Caption,
                this);
            */
        }
        catch (IOException exception)
        {
            ViewDomain.ShowErrorMessage(
                DataTableReportViewModelRes.SendNotificationOnIOException_Message,
                DataTableReportViewModelRes.SendNotificationOnIOException_Caption,
                this,
                exception);
        }
        catch (Exception exception)
        {
            ViewDomain.ShowErrorMessage(
                Res.ErrorExceuteReport_Message,
                Res.ErrorExecuteReport_Title,
                this,
                exception);
        }

        OnProcessingDone(this, new EventArgs());
        DialogResult = true;
        Close();
    }

    private bool CanSave()
    {
        return SelectedMimeType != null;
    }

    private void Design()
    {
        try
        {
            _reportEngine.OpenDesignDialog(
                ReportModel,
                ViewDomain,
                ReportLayout);
        }
        catch (Exception exception)
        {
            ViewDomain.ShowErrorMessage(
                Res.ErrorDesignReport_Message,
                Res.ErrorDesignReport_Title,
                this,
                exception);
        }
    }

    private bool CanDesign()
    {
        return IsDesignable;
    }
}