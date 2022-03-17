Imports Windows.Storage.Streams
Imports Windows.System
Imports Windows.UI.ViewManagement.Core

Public NotInheritable Class MainPage
    Inherits Page
    Private Async Sub Me_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles Me.Loaded
        Dim u As IReadOnlyList(Of User) = Await User.FindAllAsync()
        Dim current = u.Where(Function(p) p.AuthenticationStatus = UserAuthenticationStatus.LocallyAuthenticated AndAlso p.Type = UserType.LocalUser).FirstOrDefault()
        Dim data = Await current.GetPropertyAsync(KnownUserProperties.FirstName)
        Dim em = Await current.GetPropertyAsync(KnownUserProperties.AccountName)
        Dim displayName As String = CStr(data)
        Dim displayEmail As String = CStr(em)
        NameBlock.Text = "Hello " & displayName
        ContactInfo.Text = displayEmail
    End Sub
    Private Async Sub UploadButton_Click(sender As Object, e As RoutedEventArgs) Handles UploadButton.Click
        Dim open As Windows.Storage.Pickers.FileOpenPicker = New Windows.Storage.Pickers.FileOpenPicker()
        open.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        open.FileTypeFilter.Add(".rtf")
        Dim file As Windows.Storage.StorageFile = Await open.PickSingleFileAsync()
        If file IsNot Nothing Then
            Dim randAccStream As Windows.Storage.Streams.IRandomAccessStream = Await file.OpenAsync(Windows.Storage.FileAccessMode.Read)
            JournalArea.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream)
        End If
        If file Is Nothing Then
            Dim errorDialog As ContentDialog = New ContentDialog() With {
                .Title = "Open File Error",
                .Content = "The selected file is unable to be opened.",
                .PrimaryButtonText = "Ok"
            }
            Await errorDialog.ShowAsync()
        End If
    End Sub
    Private Async Sub SaveButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles SaveButton.Click
        Dim savePicker As Windows.Storage.Pickers.FileSavePicker = New Windows.Storage.Pickers.FileSavePicker()
        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        savePicker.FileTypeChoices.Add("Rich Text", New List(Of String)() From {
            ".rtf"
        })
        savePicker.SuggestedFileName = "Journal Entry"
        Dim file As Windows.Storage.StorageFile = Await savePicker.PickSaveFileAsync()
        If file IsNot Nothing Then
            Windows.Storage.CachedFileManager.DeferUpdates(file)
            Dim randAccStream As Windows.Storage.Streams.IRandomAccessStream = Await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite)
            JournalArea.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream)
            Dim status As Windows.Storage.Provider.FileUpdateStatus = Await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file)
            If status <> Windows.Storage.Provider.FileUpdateStatus.Complete Then
                Dim errorBox As Windows.UI.Popups.MessageDialog = New Windows.UI.Popups.MessageDialog("*Sniffs* Sorry, file '" & file.DisplayName & "' couldn't be saved...boo-hoo-hoo! :,(")
                Await errorBox.ShowAsync()
            End If
        End If
    End Sub
    Private Sub EmojiButton_Click(sender As Object, e As RoutedEventArgs) Handles EmojiButton.Click
        CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji)
    End Sub
End Class
