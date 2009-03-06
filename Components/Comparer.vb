Imports DotNetNuke.Entities.Users

Namespace interApps.DNN.Modules.IdentitySwitcher

	Public Enum SortBy
		DisplayName
		UserName
	End Enum

	Public Class Comparer
		Implements IComparer

		Private sortedBy As SortBy = SortBy.UserName

		Public Sub New(ByVal sortby As SortBy)
			sortedBy = sortby
		End Sub

		Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare

			Dim u1 As UserInfo = DirectCast(x, UserInfo)
			Dim u2 As UserInfo = DirectCast(y, UserInfo)

			Select Case sortedBy
				Case SortBy.DisplayName
					Return New CaseInsensitiveComparer().Compare(u1.DisplayName, u2.DisplayName)
				Case SortBy.UserName
					Return New CaseInsensitiveComparer().Compare(u1.Username, u2.Username)
			End Select

		End Function

	End Class

End Namespace