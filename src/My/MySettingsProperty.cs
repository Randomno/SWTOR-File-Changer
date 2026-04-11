// Decompiled with JetBrains decompiler
// Type: SWTOR_File_Changer.My.MySettingsProperty
// Assembly: SWTOR-File-Changer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2DD0128D-A6A1-4452-9825-73B42BC80E18
// Assembly location: E:\Projects\Mods\SWTORMods\Hack's Nude Patch\Nightcross Nude Pack\SWTOR-File-Changer.exe
// XML documentation location: E:\Projects\Mods\SWTORMods\Hack's Nude Patch\Nightcross Nude Pack\SWTOR-File-Changer.xml

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable disable
namespace SWTOR_File_Changer.My
{
  [StandardModule]
  [CompilerGenerated]
  [DebuggerNonUserCode]
  [HideModuleName]
  internal sealed class MySettingsProperty
  {
    [HelpKeyword("My.Settings")]
    internal static MySettings Settings
    {
      get
      {
        MySettings settings = MySettings.Default;
        return settings;
      }
    }
  }
}
