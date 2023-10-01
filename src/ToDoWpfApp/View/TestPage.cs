using Kapok.View;

namespace ToDoWpfApp.View;

public class TestPage : InteractivePage
{
    public TestPage(IViewDomain viewDomain)
        : base(viewDomain)
    {
        Title = "Test page";
    }
}