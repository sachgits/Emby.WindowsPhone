﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediaBrowser.ApiInteraction;
using MediaBrowser.WindowsPhone.Model;
using MediaBrowser.Model.Dto;
using MediaBrowser.WindowsPhone.Resources;
using ScottIsAFool.WindowsPhone.IsolatedStorage;
using MediaBrowser.Shared;

namespace MediaBrowser.WindowsPhone.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ChooseProfileViewModel : ViewModelBase
    {
        private readonly ExtendedApiClient ApiClient;
        private readonly INavigationService NavigationService;

        /// <summary>
        /// Initializes a new instance of the ChooseProfileViewModel class.
        /// </summary>
        public ChooseProfileViewModel(ExtendedApiClient apiClient, INavigationService navigationService)
        {
            ApiClient = apiClient;
            NavigationService = navigationService;
            Profiles = new ObservableCollection<UserDto>();
            if(IsInDesignMode)
            {
                Profiles = new ObservableCollection<UserDto>()
                               {
                                   new UserDto
                                       {
                                           Id = new Guid("dd425709431649698e92d86b1f2b00fa").ToString(),
                                           Name = "ScottIsAFool"
                                       },
                                   new UserDto
                                       {
                                           Id = new Guid("dab28e40cfbc43658082f55a44cf139a").ToString(),
                                           Name = "Redshirt",
                                           LastLoginDate = DateTime.Now.AddHours(-1)
                                       }
                               };
            }
            else
            {
                WireCommands();
            }
        }

        private void WireCommands()
        {
            ChooseProfilePageLoaded = new RelayCommand(async ()=>
            {
                if(NavigationService.IsNetworkAvailable)
                {
                    ProgressText = AppResources.SysTrayGettingProfiles;
                    ProgressIsVisible = true;
                    var profiles = await ApiClient.GetAllUsersAsync();
                    foreach(var profile in profiles)
                        Profiles.Add(profile);
                    ProgressText = string.Empty;
                    ProgressIsVisible = false;
                }
            });

            LoginCommand = new RelayCommand<object[]>(async loginDetails =>
            {
                var selectedUser = loginDetails[0] as UserDto;
                var pinCode = loginDetails[1] as string;
                var saveUser = (bool)loginDetails[2];

                if (selectedUser != null)
                {

                    Debug.WriteLine(selectedUser.Id);
                    ProgressText = AppResources.SysTrayAuthenticating;
                    ProgressIsVisible = true;

                    await Utils.Login(selectedUser, pinCode, () =>
                    {
                        SetUser(selectedUser);
                        if(saveUser)
                        {
                            ISettings.SetKeyValue(Constants.SelectedUserSetting, new UserSettingWrapper
                                                                                     {
                                                                                         User = selectedUser,
                                                                                         Pin = pinCode
                                                                                     });
                        }
                    });

                    ProgressText = "";
                    ProgressIsVisible = false;
                }
            });
        }

        private void SetUser(UserDto profile)
        {
            App.Settings.LoggedInUser = profile;
            if (!string.IsNullOrEmpty(App.Action))
                NavigationService.NavigateToPage(App.Action);
            else
                NavigationService.NavigateToPage("/Views/MainPage.xaml");
        }

        public string ProgressText { get; set; }
        public bool ProgressIsVisible { get; set; }

        public ObservableCollection<UserDto> Profiles { get; set; }

        public RelayCommand ChooseProfilePageLoaded { get; set; }
        public RelayCommand<object[]> LoginCommand { get; set; }
    }
}