using Kapok.Report;
using Kapok.Report.Model;
using Res = Kapok.View.Wpf.Report.Resources.LinqQueryableReportViewModel;

namespace Kapok.View.Wpf.Report;

public sealed class LinqQueryableReportPage : DataTableReportPage<LinqQueryableReportProcessor, LinqQueryableReport>
{
    static LinqQueryableReportPage()
    {
        WpfViewDomain.RegisterPageWpfWindowConstructor<LinqQueryableReportPage>(() => new LinqQueryableReportWindow());
    }

    public LinqQueryableReportPage(LinqQueryableReport model, IViewDomain viewDomain)
        : base(model, new LinqQueryableReportProcessor(), viewDomain)
    {
    }

    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global
}