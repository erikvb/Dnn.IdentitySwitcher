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
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Entities.Profile
Imports DotNetNuke.Security.Roles

Namespace interApps.DNN.Modules.IdentitySwitcher

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The ViewDynamicModule class displays the content
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Partial Class ViewIdentitySwitcher
        Inherits Entities.Modules.PortalModuleBase

#Region " Private Properties"

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

        Private ReadOnly Property UseAjax() As Boolean
            Get
                Dim bRetValue As Boolean = True
                If Settings.Contains("useAjax") Then
                    Boolean.TryParse(Settings("useAjax"), bRetValue)
                End If
                Return bRetValue
            End Get
        End Property
#End Region


#Region " Private Methods"

        Private Function AddSearchItem(ByVal name As String) As ListItem
            Dim propertyName As String = Null.NullString
            If Not Request.QueryString("filterProperty") Is Nothing Then
                propertyName = Request.QueryString("filterProperty")
            End If

            Dim text As String = Localization.GetString(name, Me.LocalResourceFile)
            If text = "" Then text = name
            Dim li As ListItem = New ListItem(text, name)
            If name = propertyName Then
                li.Selected = True
            End If
            Return li
        End Function

        Private Sub BindSearchOptions()
            ddlSearchType.Items.Add(AddSearchItem("RoleName"))
            ddlSearchType.Items.Add(AddSearchItem("Email"))
            ddlSearchType.Items.Add(AddSearchItem("Username"))
            Dim profileProperties As ProfilePropertyDefinitionCollection = ProfileController.GetPropertyDefinitionsByPortal(PortalId, False)

            For Each definition As ProfilePropertyDefinition In profileProperties
                ddlSearchType.Items.Add(AddSearchItem(definition.PropertyName))
            Next
        End Sub

        Private Sub LoadDefaultUsers()
            If IncludeHostUser Then
                Dim arHostUsers As ArrayList = UserController.GetUsers(Null.NullInteger)
                For Each hostUser As UserInfo In arHostUsers
                    cboUsers.Items.Insert(0, New ListItem(hostUser.Username, hostUser.UserID))
                Next
            End If
            cboUsers.Items.Insert(0, New ListItem(Localization.GetString("Anonymous", LocalResourceFile), Null.NullInteger.ToString))
        End Sub

        Private Sub LoadAllUsers()
            cboUsers.DataSource = UserController.GetUsers(PortalId)
            cboUsers.DataBind()

            LoadDefaultUsers()
        End Sub

        Private Sub Filter(ByVal SearchText As String, ByVal SearchField As String)
            Dim users As ArrayList
            Dim total As Integer = 0

            Select Case SearchField
                Case "Email"
                    users = UserController.GetUsersByEmail(PortalId, False, SearchText + "%", -1, -1, total)
                Case "Username"
                    users = UserController.GetUsersByUserName(PortalId, False, SearchText + "%", -1, -1, total)
                Case "RoleName"
                    Dim objRolecontroller As New RoleController
                    users = objRolecontroller.GetUsersByRoleName(PortalId, SearchText)

                Case Else
                    users = UserController.GetUsersByProfileProperty(PortalId, False, SearchField, SearchText + "%", 0, 1000, total)
            End Select
            cboUsers.DataSource = users
            cboUsers.DataBind()

            LoadDefaultUsers()
        End Sub
#End Region


#Region "Event Handlers"

        ''' <summary>
        ''' Runs when the page loads. Databinds the user switcher drop down list.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            Try
                If UseAjax AndAlso Framework.AJAX.IsInstalled Then
                    Framework.AJAX.RegisterScriptManager()

                    Framework.AJAX.CreateUpdateProgressControl(Me.UpdatePanel1.ID)
                End If
                If Not Page.IsPostBack Then

                    BindSearchOptions()
                    LoadAllUsers()

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

        Protected Sub cmdSearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdSearch.Click
            If txtSearch.Text = "" Then
                LoadAllUsers()
            Else
                Filter(txtSearch.Text, ddlSearchType.SelectedValue)
            End If
        End Sub

        Protected Sub cmdSwitch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdSwitch.Click
            If (cboUsers.SelectedValue <> Me.UserId) Then
                If (cboUsers.SelectedValue = Null.NullInteger.ToString) Then
                    Response.Redirect(NavigateURL("LogOff"))
                Else
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

            End If
        End Sub



#End Region


    End Class

End Namespace
