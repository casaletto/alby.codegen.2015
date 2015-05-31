using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;
namespace alby.codegen.runtime
{
	public class CodeGenEtc
	{
		protected static int	__TIMEOUT = 60 * 60; // 1 hour default timeout
		protected static string __sql = "";
		protected static bool	__debugSql = false;

		public static bool DebugSql
		{
			get
			{
				return __debugSql;
			}
			set
			{
				__debugSql = value;
			}
		}

		public static string Sql
		{
			get
			{
				return __sql;
			}
			set
			{
				__sql = value ?? "" ;
				if ( __debugSql )
					if ( __sql.Length > 0 )
						DebugMessage( __sql );
			}
		}

		public static int Timeout
		{
			get
			{
				return __TIMEOUT;
			}
			set
			{
				__TIMEOUT = value;
			}
		}

		public static void ConsoleMessage(bool show, string fmt, params object[] args)
		{
			if (show)
				ConsoleMessage(fmt, args);
		}

		public static void ConsoleMessage(string fmt, params object[] args)
		{
			Console.WriteLine(fmt, args);
			//DebugMessage(fmt, args);
		}

		public static void DebugMessage(bool show, string fmt, params object[] args)
		{
			if (show)
				DebugMessage(fmt, args);
		}

		public static void DebugMessage(string fmt, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(fmt, args);
		}

	}
}
