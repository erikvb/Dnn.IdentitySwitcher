'
' Copyright (c) 2008, Erik van Ballegoij, Apollo Software 
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

Imports DotNetNuke
Imports System.Web.UI
Imports System.Collections.Generic
Imports System.Reflection

Namespace Apollo.DNN.Modules.UserSwitcher

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The ViewDynamicModule class displays the content
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Partial Class ViewUserSwitcher
        Inherits Entities.Modules.PortalModuleBase

        ''' <summary>
        ''' reads the setting for inclusion of the host user. This setting defaults to false
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property IncludeHostUser() As Boolean
            Get
                Dim bRetValue As Boolean = False
                If Settings.Contains("includeHost") Then
                    Boolean.TryParse(Settings("includeHost"), bRetValue)
                End If
                Return bRetValue
            End Get
        End Property

#Region "Event Handlers"

        ''' <summary>
        ''' Runs when the page loads. Databinds the user switcher drop down list.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Try
                If Not Page.IsPostBack Then
                    cboUsers.DataSource = UserController.GetUsers(PortalId)
                    cboUsers.DataTextField = "UserName"
                    cboUsers.DataValueField = "UserId"
                    cboUsers.DataBind()

                    If IncludeHostUser Then
                        Dim arHostUsers As ArrayList = UserController.GetUsers(Null.NullInteger)
                        For Each hostUser As UserInfo In arHostUsers
                            cboUsers.Items.Insert(0, New ListItem(hostUser.Username, hostUser.UserID))
                        Next
                    End If
                    cboUsers.Items.Insert(0, New ListItem(Localization.GetString("NoneSelected", LocalResourceFile), Null.NullInteger.ToString))

                    If (Not IncludeHostUser) And UserInfo.IsSuperUser Then
                        cboUsers.SelectedIndex = 0
                    Else
                        cboUsers.SelectedValue = UserInfo.UserID
                    End If
                End If
            Catch exc As Exception        'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub


        ''' <summary>
        ''' performs logon for the selected user
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Sub cboUsers_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboUsers.SelectedIndexChanged
            If (cboUsers.SelectedValue <> Me.UserId) AndAlso (cboUsers.SelectedValue <> Null.NullInteger.ToString) Then
                Dim MyUserInfo As UserInfo = UserController.GetUser(PortalId, cboUsers.SelectedValue, False)
                If Not MyUserInfo Is Nothing Then
                    'Remove user from cache
                    If Page.User IsNot Nothing Then
                        DataCache.ClearUserCache(Me.PortalSettings.PortalId, Context.User.Identity.Name)
                    End If

                    ' sign current user out
                    Dim objPortalSecurity As New PortalSecurity
                    objPortalSecurity.SignOut()

                    ' sign new user in
                    UserController.UserLogin(PortalId, MyUserInfo, PortalSettings.PortalName, Request.UserHostAddress, False)

                    ' redirect to current url
                    Response.Redirect(Request.RawUrl, True)
                End If

            End If
        End Sub


#End Region

    End Class

End Namespace
