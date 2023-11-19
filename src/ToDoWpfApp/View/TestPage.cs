using System;
using Kapok.View;

namespace ToDoWpfApp.View;

public class TestPage : InteractivePage
{
    public TestPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        Title = "Test page";
    }
}