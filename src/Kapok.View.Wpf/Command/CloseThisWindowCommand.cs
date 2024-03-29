﻿using System.Windows.Input;

namespace Kapok.View.Wpf;

public class CloseThisWindowCommand : ICommand
{
    #region ICommand Members

    public bool CanExecute(object parameter)
    {
        //we can only close Windows
        return (parameter is System.Windows.Window);
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
        if (CanExecute(parameter))
        {
            ((System.Windows.Window)parameter).Close();
        }
    }

    #endregion

    private CloseThisWindowCommand()
    {

    }

    public static readonly ICommand Instance = new CloseThisWindowCommand();
}