using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileEditor
{
	public enum MatchPattern
	{
		Equals,
		StartsWith,
		Contains,
	}

	string _mFilePath = null;
	List<string> _mLines = new List<string>();
	int _mCursor = 0;

	public FileEditor(string filePath)
	{
		_mFilePath = filePath;
		if( !File.Exists( _mFilePath ) ) 
		{
			return;
		}

		StreamReader streamReader = new StreamReader(filePath);
		string line = null;
		while ((line = streamReader.ReadLine ()) != null) 
		{
			_mLines.Add (line);
		}
		streamReader.Close ();

		_mCursor = _mLines.Count > 1 ? 0 : -1;
	}

	public int Find(string target, MatchPattern pattern = MatchPattern.Equals, bool foward = true)
	{
		int i = _mCursor;

		while (i >= 0 && i < _mLines.Count)
		{
			if (pattern == MatchPattern.Equals && _mLines [i].Trim ().Equals (target)
				|| pattern == MatchPattern.Contains && _mLines [i].Trim ().Contains (target)
				|| pattern == MatchPattern.StartsWith && _mLines [i].Trim().StartsWith(target))
			{
				_mCursor = i;
				return _mCursor;
			}

			i = foward ? i + 1 : i - 1;
		}

		return -1;
	}

	public bool Contains(string target)
	{
		if (_mCursor >= 0 && _mCursor < _mLines.Count)
		{
			return _mLines [_mCursor].Contains (target);
		}
		return false;
	}

	/// <summary>
	/// appends a line after current cursor, the cursor will also move to the next line
	/// </summary>
	/// <param name="lineToInsert">Line to insert.</param>
	public void Append(string lineToAppend, bool followIndent = true)
	{
		string indent = "";
		if (_mCursor >= 0 && followIndent) 
		{
			string curLine = _mLines [_mCursor];
			indent = curLine.Substring (0, curLine.IndexOf (curLine.TrimStart ()));
		}

		if (_mCursor + 1 > _mLines.Count - 1)
		{
			_mLines.Add (indent + lineToAppend);
		}
		else
		{
			_mLines.Insert (_mCursor + 1, indent + lineToAppend);
		}
		++_mCursor;
	}

	public void Insert(string lineToInsert, bool followIndent = true)
	{
		string indent = "";
		if (_mCursor >= 0 && followIndent) 
		{
			string curLine = _mLines [_mCursor];
			indent = curLine.Substring (0, curLine.IndexOf (curLine.TrimStart ()));
		}

		if (_mCursor < 0)
		{
			_mLines.Add (indent + lineToInsert);
		} 
		else
		{
			_mLines.Insert (_mCursor, indent + lineToInsert);
		}
	}

	public void DeleteLine()
	{
		if (_mCursor < 0)
			return;
		
		_mLines.RemoveAt (_mCursor);
	}

	public void ReplaceLine(string lineToReplace)
	{
		Append (lineToReplace);
		--_mCursor;
		DeleteLine ();
	}

	public void ReplaceSubString(string target, string replacement)
	{
		if (_mCursor < 0) 
		{
			return;
		}

		string curLine = _mLines [_mCursor];
		_mLines [_mCursor] = curLine.Replace (target, replacement);
	}

	public void Save()
	{
		StreamWriter streamWriter = new StreamWriter (_mFilePath);
		foreach (string line in _mLines)
		{
			streamWriter.WriteLine (line);
		}
		streamWriter.Close ();
	}
}
