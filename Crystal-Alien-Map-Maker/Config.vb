﻿Imports System.IO
Imports Nini.Config
Imports Nini.Ini

Public Module Config

    Public Function LoadConfig() As Boolean
        AsciiLookup = New List(Of Char)()

        TileDefs = New TileDef() {}
        BuildingDefs = New BuildingDef() {}
        UnitDefs = New UnitDef() {}

        RectangleBrushPresets = New List(Of RectangleBrushPreset)()

        If Not CheckFileExists(ConfigFile, ConfigFileName) Then
            Return False
        ElseIf Not CheckFileExists(TerrainFile, TerrainFileName) Then
            Return False
        ElseIf Not CheckFileExists(BuildingsFile, BuildingsFileName) Then
            Return False
        ElseIf Not CheckFileExists(UnitsFile, UnitsFileName) Then
            Return False
        End If

        Dim reader As IniReader
        Dim source As IniConfigSource
        Dim config As IConfig

        reader = New IniReader(ConfigFile) With {.IgnoreComments = True, .AcceptCommentAfterKey = False}
        source = New IniConfigSource(New IniDocument(reader))
        config = source.Configs.Item("CAMM")

        If Not CheckFileFormat(ConfigFileName, ConfigFormat, config) Then
            reader.Close()
            Return False
        End If

        config = source.Configs.Item("ASCII LOOKUP")

        AsciiSeparator = config.GetString("Ascii Separator")
        Dim ascii As String() = config.Get("Ascii Array").Trim(IniArray.ToCharArray).Trim(AsciiSeparator.ToCharArray()).Split(New String() {AsciiSeparator}, StringSplitOptions.None)
        For Each str As String In ascii
            AsciiLookup.Add(Char.Parse(str))
        Next

        reader.Close()
        reader = New IniReader(TerrainFile) With {.IgnoreComments = True}
        source = New IniConfigSource(New IniDocument(reader))
        config = source.Configs.Item("CAMM")

        If Not CheckFileFormat(TerrainFileName, TerrainFormat, config) Then
            reader.Close()
            Return False
        End If

        config = source.Configs.Item("DEFINE TERRAIN")

        For i As Integer = 0 To config.GetKeys().Length - 1
            Dim keyName As String = "Terrain" + i.ToString()
            If config.Get(keyName, "-1") <> "-1" Then
                Dim keyArray As String() = config.Get(keyName).Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)
                Dim tileId As Integer = CInt(keyArray(0))
                Dim isPassable As Boolean = CBool(keyArray(1))
                Dim isMinerals As Boolean = CBool(keyArray(2))
                Dim imageUrl As String = keyArray(3)

                Dim fullImageUrl As String = DataPath + "/" + imageUrl
                Dim theImage As Image = Image.FromFile(fullImageUrl)

                TileImageLookup.Add(tileId, theImage)

                ReDim Preserve TileDefs(i)
                TileDefs(i) = New TileDef(tileId, isPassable, isMinerals, imageUrl)
            End If
        Next

        reader.Close()
        reader = New IniReader(BuildingsFile) With {.IgnoreComments = True}
        source = New IniConfigSource(New IniDocument(reader))
        config = source.Configs.Item("CAMM")

        If Not CheckFileFormat(BuildingsFileName, BuildingsFormat, config) Then
            reader.Close()
            Return False
        End If

        config = source.Configs.Item("DEFINE BUILDINGS")

        For i As Integer = 0 To config.GetKeys().Length - 1
            Dim keyName As String = "Building" + i.ToString()
            If config.Get(keyName, "-1") <> "-1" Then
                Dim keyArray As String() = config.Get(keyName).Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)
                Dim buildingId As String = keyArray(0)
                Dim width As Integer = CInt(keyArray(1))
                Dim height As Integer = CInt(keyArray(2))
                Dim team As Team = CType(Integer.Parse(keyArray(3)), Team)
                Dim offsetY As Integer = CInt(keyArray(4))
                Dim imageUrl As String = keyArray(5)
                Dim shadowImageUrl As String = keyArray(6)

                Dim fullImageUrl As String = DataPath + "/" + imageUrl
                Dim fullShadowImageUrl As String = DataPath + "/" + shadowImageUrl

                Dim test As Bitmap = Bitmap.FromFile(fullImageUrl)
                Dim thumbnail As New Bitmap(TileSizeX, TileSizeY)
                Dim bitX, bitY As Integer
                For bitX = 0 To thumbnail.Width - 1
                    For bitY = 0 To thumbnail.Height - 1
                        thumbnail.SetPixel(bitX, bitY, test.GetPixel(bitX + ((test.Width / 2) - (TileSizeX / 2)), bitY + ((test.Height / 2) - TileSizeY) + offsetY))
                    Next
                Next
                'Dim TheImage As Image = thumbnail
                'TheImage = TheImage.GetThumbnailImage(TileSizeX, TileSizeY, Nothing, System.IntPtr.Zero)

                BuildingSmallImageLookup.Add(buildingId, thumbnail)
                BuildingFullImageLookup.Add(buildingId, Image.FromFile(fullImageUrl))
                BuildingShadowImageLookup.Add(buildingId, Image.FromFile(fullShadowImageUrl))

                ReDim Preserve BuildingDefs(i)
                BuildingDefs(i) = New BuildingDef(buildingId, width, height, team, offsetY, imageUrl, shadowImageUrl)
            End If
        Next

        reader.Close()
        reader = New IniReader(UnitsFile) With {.IgnoreComments = True}
        source = New IniConfigSource(New IniDocument(reader))
        config = source.Configs.Item("CAMM")

        If Not CheckFileFormat(UnitsFileName, UnitsFormat, config) Then
            reader.Close()
            Return False
        End If

        config = source.Configs.Item("DEFINE UNITS")

        For i As Integer = 0 To config.GetKeys().Length - 1
            Dim keyName As String = "Unit" + i.ToString()
            If config.Get(keyName, "-1") <> "-1" Then
                Dim keyArray As String() = config.Get(keyName).Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)
                Dim unitId As String = keyArray(0)
                Dim team As Team = CType(Integer.Parse(keyArray(1)), Team)
                Dim altitude As Integer = Integer.Parse(keyArray(2))
                Dim isPickup As Boolean = Boolean.Parse(keyArray(3))
                Dim offsetY As Integer = Integer.Parse(keyArray(4))
                Dim imageUrl As String = keyArray(5)
                Dim shadowImageUrl As String = keyArray(6)

                Dim fullImageUrl As String = DataPath + "/" + imageUrl
                Dim fullShadowImageUrl As String = DataPath + "/" + shadowImageUrl

                Dim test As Bitmap = Bitmap.FromFile(fullImageUrl)
                Dim w As Integer = TileSizeX
                Dim h As Integer = TileSizeY
                If test.Size.Width < TileSizeX Then
                    w = test.Size.Width
                End If
                If test.Size.Height < TileSizeY Then
                    h = test.Size.Height
                End If

                Dim thumbnail As New Bitmap(w, h)
                Dim bitX, bitY As Integer
                For bitX = 0 To thumbnail.Width - 1
                    For bitY = 0 To thumbnail.Height - 1
                        Dim pixel As Color = Color.Transparent
                        Try
                            pixel = test.GetPixel(bitX + ((test.Width / 2) - (w / 2)), bitY + ((test.Height / 2) - TileSizeY) + offsetY)
                        Catch ex As Exception

                        End Try
                        thumbnail.SetPixel(bitX, bitY, pixel)
                    Next
                Next
                'Dim TheImage As Image = thumbnail
                'TheImage = TheImage.GetThumbnailImage(TileSizeX, TileSizeY, Nothing, System.IntPtr.Zero)

                UnitSmallImageLookup.Add(unitId, thumbnail)
                UnitFullImageLookup.Add(unitId, Image.FromFile(fullImageUrl))
                UnitShadowImageLookup.Add(unitId, Image.FromFile(fullShadowImageUrl))

                ReDim Preserve UnitDefs(i)
                UnitDefs(i) = New UnitDef(unitId, team, altitude, isPickup, offsetY, imageUrl, shadowImageUrl)
            End If
        Next

        If My.Computer.FileSystem.DirectoryExists(RectangleBrushPath) Then
            Dim files As FileInfo() = My.Computer.FileSystem.GetDirectoryInfo(RectangleBrushPath).GetFiles()
            For Each file As FileInfo In files
                reader.Close()
                reader = New IniReader(file.FullName) With {.IgnoreComments = True}
                source = New IniConfigSource(New IniDocument(reader))
                config = source.Configs.Item("RectPreset")

                Dim row1 As String() = config.Get("Row1").Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)
                Dim row2 As String() = config.Get("Row2").Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)
                Dim row3 As String() = config.Get("Row3").Trim(IniArray.ToCharArray).Split(New Char() {IniSeparator}, StringSplitOptions.None)

                Dim data As Integer()() = New Integer(2)() {}
                data(0) = New Integer() {row1(0), row2(0), row3(0)}
                data(1) = New Integer() {row1(1), row2(1), row3(1)}
                data(2) = New Integer() {row1(2), row2(2), row3(2)}

                RectangleBrushPresets.Add(New RectangleBrushPreset(file.Name, config.GetString("Title"), data))
            Next
        End If

        reader.Close()

        Return True
    End Function

    Private Function CheckFileExists(filePath As String, fileName As String) As Boolean
        If My.Computer.FileSystem.FileExists(filePath) Then
            Return True
        Else
            MsgBox("The file '" + fileName + "' is missing!" + vbNewLine + "Please make sure you have all required files before using CAMM.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Return False
        End If
    End Function

    Private Function CheckFileFormat(fileName As String, currentFormat As Integer, config As IConfig) As Boolean
        If config Is Nothing Then
            MsgBox("The file '" + fileName + "' is invalid or outdated and cannot be used!", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Return False
        End If

        Dim format As Integer = config.GetInt("Format", -1)

        If format < currentFormat Then
            MsgBox("The file '" + fileName + "' is invalid or outdated and cannot be used!", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Return False
        ElseIf format > currentFormat Then
            MsgBox("The file '" + fileName + "' was created for a newer version of CAMM and cannot be used!", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly)
            Return False
        Else
            Return True
        End If
    End Function

    'ASCII Characters Separator
    Public AsciiSeparator As String

    'ASCII Lookup Array
    Public AsciiLookup As List(Of Char)

    'Tile Definitions.
    Public TileDefs() As TileDef

    'Building Definitions.
    Public BuildingDefs() As BuildingDef

    'Unit Definitions.
    Public UnitDefs() As UnitDef

    'Rectangle Brush Preset Definitions.
    Public RectangleBrushPresets As List(Of RectangleBrushPreset)

End Module
