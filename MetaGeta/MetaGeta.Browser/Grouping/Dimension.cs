// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.
//<Serializable()> _
//Public Class Dimension
//    Private m_Items As New Dictionary(Of String, Bucket)

//    Private m_Name As String

//    Public Sub New(ByVal name As String)
//        m_Name = name
//    End Sub

//    Public ReadOnly Property Name() As String
//        Get
//            Return m_Name
//        End Get
//    End Property

//    Public ReadOnly Property Item(ByVal tagValue As String) As Bucket
//        Get
//            Dim b As Bucket = Nothing
//            If Not m_Items.TryGetValue(tagValue, b) Then
//                b = New Bucket(New MGTag(m_Name, tagValue))
//                m_Items.Add(tagValue, b)
//            End If
//            Return b
//        End Get
//    End Property

//    Public Sub IndexNewItem(ByVal file As MGFile)
//        Me.Item(file.Tags.Item(m_Name).Value).Add(file)
//    End Sub
//End Class
