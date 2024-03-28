global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections;
global using System.Collections.ObjectModel;
global using System.Windows.Input;

global using Microsoft.Maui.Graphics;

global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Messaging;
global using CommunityToolkit.Mvvm.Messaging.Messages;

global using Microsoft.EntityFrameworkCore;
global using EFCore.BulkExtensions;
global using Acr.UserDialogs;
global using Microsoft.Extensions.Logging;

// ___ MySolarCells -----------
global using MySolarCells.Messages;
global using MySolarCells.Resources.Translations;
global using MySolarCells.Helpers;
global using MySolarCells.Resources.Styles;
global using MySolarCells.Models;
//Views
global using MySolarCells.Views;
global using MySolarCells.Views.OnBoarding;
global using MySolarCells.Views.Roi;
global using MySolarCells.Views.Energy;
global using MySolarCells.Views.More;
//ViewModels
global using MySolarCells.ViewModels;
global using MySolarCells.ViewModels.OnBoarding;
global using MySolarCells.ViewModels.Roi;
global using MySolarCells.ViewModels.Energy;
global using MySolarCells.ViewModels.More;
//Service
global using MySolarCells.Services;
global using MySolarCells.Services.Inverter;
global using MySolarCells.Services.GridSupplier;

//SQLITE