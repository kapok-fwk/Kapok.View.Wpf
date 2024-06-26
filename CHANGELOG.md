# Changelog

## 0.2.0 (204-04-13)

- dependency upgrade: renamed Dao to EntityService

## 0.1.22 (2023-12-03)

- :rocket: build .NET 8.0 assemblies (finalize work of 0.1.21)

## 0.1.21 (2023-11-19)

- :rocket: support .Net 8.0 build
- :rocket: upgrade to Kapok 0.1.21

## 0.1.17 (2023-11-05)

- :rocket: support .net 7.0 build
- :rocket: upgrade to Kapok 0.1.17
- :bug: make sure not binding to null in ImageCommand

## 0.1.10.1 (2023-10-10)

- :bug: *fix* support clipboard pasting to nullable value types

## 0.1.10 (2023-10-10)

- :change: ensure no memory leak by WpfViewDomain._pageContainer
- :bug: *fix* adding decimal type #1 with copy/past

## 0.1.9 (2023-10-01)

- :rocket: upgrade packages
- :bug: *fixing* focus issues with AvalonDock with tree view navigation menu

## 0.1.8.2 (2023-09-02)

- :rocket: change show QuestionDialog in taskbar
- :bug: *fix* nullable crash with CustomDataGrid filter

## 0.1.8.1 (2023-08-27)

- :rocket: enable filter on DataGrid filter bar possible when pressing key Enter
- :bug: *fix* tab navigation issue in DataGrid filter bar
- :bug: *fix* crash when setting filter via FilterSet UI control

## 0.1.8 (2023-08-13)

- :tada: add support for ColumnPropertyView.TextWrap
- :rocket: dependency upgrade
- :rocket: upgrade AvalonDock dependecy to 4.72.0
- :bug: *fix* refactoring internal logic to get ContentControl class

## 0.1.5.8 (2023-06-10)

- :rocket: dependency upgrade

## 0.1.5.6 (2023-05-21)

- :rocket: *change* integrate MimeTypeMap internally, drop project MimeTypeMap (#5)
- :rocket: dependency upgrade

## 0.1.5 (2023-04-14)

- :rocket: dependency upgrade

## 0.1.4 (2023-02-28)

- :tada: *feat* support silent authentication from Kapok.Acl.Windows.WindowsLocalUserAuthenticationService close #2
- :tada: *feat* add support for RibbonMenuButton with sub-menu-items to the 4th level
- :tada: *feat* add calendar images
- :tada: *feat* add support for Menus in Toolbar (including PopupListPage)

- :dizzy: *UX* improve EditDataGridFilter design
- :dizzy: *UX* improve MimeTypeReportPageWindow layout

- :rocket: *change* use optimistic behavior in IActionToICommandConverter
- :rocket: dependency upgrade

- :bug: *fix* resource reference when using in source in other projects
- :bug: *fix* button translation
- :bug: *fix* report parameter binding

## 0.1.2 (2022-05-15)

- :bug: *fix* application crash "Value cannot be null. (Parameter 'element')" when changing a ListView of a ListPage while editing an entity in a CustomDataGrid control

## 0.1.1 (2022-04-23)

- :tada: *feat* add strong name to libraries

## 0.1.0 (2022-04-20)

- initial version
