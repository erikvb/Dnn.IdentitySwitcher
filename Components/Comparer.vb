'
' Copyright (c) 2008 - 2009, interApps, Erik van Ballegoij, http://www.interapps.nl
' All rights reserved.
'
' Redistribution and use in source and binary forms, with or without modification, are permitted provided that the 
' following conditions are met:
'
' * Redistributions of source code must retain the above copyright notice, this list of conditions and the 
'   following disclaimer.
' * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the 
'   following disclaimer in the documentation and/or other materials provided with the distribution.
' * Neither the name of Apollo Software nor the names of its contributors may be used to endorse or promote products 
'  derived from this software without specific prior written permission.
'
' THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
' INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
' DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
' SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
' SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
' WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
' USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'

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