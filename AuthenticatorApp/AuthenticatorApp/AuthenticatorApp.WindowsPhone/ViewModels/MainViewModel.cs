using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticatorApp.Models;
using GalaSoft.MvvmLight;

namespace AuthenticatorApp.ViewModels
{
    partial class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            AccountListCollection = new ObservableCollection<Account>();
        }

        private ObservableCollection<Account> _accountListCollection;

        public ObservableCollection<Account> AccountListCollection
        {
            get { return _accountListCollection; }
            set
            {
                if (_accountListCollection == value) return;
                _accountListCollection = value;
                RaisePropertyChanged();
            }
        } 
    }
}
