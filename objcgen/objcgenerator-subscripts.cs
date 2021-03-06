﻿using System;

using IKVM.Reflection;
using Type = IKVM.Reflection.Type;

using Embeddinator;

namespace ObjC {

	public partial class ObjCGenerator {

		protected void GenerateSubscript (ProcessedProperty property)
		{
			PropertyInfo pi = property.Property;

			bool hasRead = pi.GetGetMethod () != null;
			bool hasWrite = pi.GetSetMethod () != null;

			Type indexType = hasWrite ? pi.GetSetMethod ().GetParameters ()[0].ParameterType : pi.GetGetMethod ().ReturnType;
			Type paramType = pi.PropertyType;
			switch (Type.GetTypeCode (indexType)) {
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
				if (hasRead)
					GenerateIndexedSubscriptingRead (indexType, paramType);
				if (hasWrite)
					GenerateIndexedSubscriptingWrite (indexType, paramType);
				return;
			default:
				if (hasRead)
					GenerateKeyedSubscriptingRead (indexType, paramType);
				if (hasWrite)
					GenerateKeyedSubscriptingWrite (indexType, paramType);
				return;
			}
		}

		protected void GenerateKeyedSubscriptingRead (Type indexType, Type propertyType)
		{
			string indexTypeString = NameGenerator.GetTypeName (indexType);

			// TODO - Technically the argument here can be anything, not just id
			headers.WriteLine ($"- (id)objectForKeyedSubscript:(id)key;");

			implementation.WriteLine ($"- (id)objectForKeyedSubscript:(id)key;");
			implementation.WriteLine ("{");
			implementation.Indent++;
			implementation.WriteLine ($"return {ToNSObject (propertyType, "[self getItem:key]")};");
			implementation.Indent--;
			implementation.WriteLine ("}");
			implementation.WriteLine ();
		}

		protected void GenerateKeyedSubscriptingWrite (Type indexType, Type propertyType)
		{
			// TODO - Technically the argument here can be anything, not just id
			headers.WriteLine ($"- (void)setObject:(id)obj forKeyedSubscript:(id)key;");

			implementation.WriteLine ($"- (void)setObject:(id)obj forKeyedSubscript:(id)key;");
			implementation.WriteLine ("{");
			implementation.Indent++;
			implementation.WriteLine ($"[self setItem:key value:{FromNSObject (propertyType, "obj")}];");
			implementation.Indent--;
			implementation.WriteLine ("}");
			implementation.WriteLine ();
		}

		protected void GenerateIndexedSubscriptingRead (Type indexType, Type propertyType)
		{
			string indexTypeString = NameGenerator.GetTypeName (indexType);

			headers.WriteLine ($"- (id)objectAtIndexedSubscript:({indexTypeString})idx;");

			implementation.WriteLine ($"- (id)objectAtIndexedSubscript:({indexTypeString})idx");
			implementation.WriteLine ("{");
			implementation.Indent++;
			implementation.WriteLine ($"return {ToNSObject (propertyType, "[self getItem:idx]")};");
			implementation.Indent--;
			implementation.WriteLine ("}");
			implementation.WriteLine ();
		}

		protected void GenerateIndexedSubscriptingWrite (Type indexType, Type propertyType)
		{
			string indexTypeString = NameGenerator.GetTypeName (indexType);

			headers.WriteLine ($"- (void)setObject:(id)obj atIndexedSubscript:({indexTypeString})idx;");

			implementation.WriteLine ($"- (void)setObject:(id)obj atIndexedSubscript:({indexTypeString})idx");
			implementation.WriteLine ("{");
			implementation.Indent++;
			implementation.WriteLine ($"[self setItem:idx value:{FromNSObject (propertyType, "obj")}];");
			implementation.Indent--;
			implementation.WriteLine ("}");
			implementation.WriteLine ();
		}

		internal string ToNSObject (Type type, string code)
		{
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Boolean:
				return $"[[NSNumber alloc] initWithBool: {code}]";
			case TypeCode.SByte:
				return $"[[NSNumber alloc] initWithChar: {code}]";
			case TypeCode.Byte:
				return $"[[NSNumber alloc] initWithUnsignedChar: {code}]";
			case TypeCode.Int16:
				return $"[[NSNumber alloc] initWithShort: {code}]";
			case TypeCode.Char:
			case TypeCode.UInt16:
				return $"[[NSNumber alloc] initWithUnsignedShort: {code}]";
			case TypeCode.Int32:
				return $"[[NSNumber alloc] initWithInt: {code}]";
			case TypeCode.UInt32:
				return $"[[NSNumber alloc] initWithUnsignedInt: {code}]";
			case TypeCode.Int64:
				return $"[[NSNumber alloc] initWithLongLong: {code}]";
			case TypeCode.UInt64:
				return $"[[NSNumber alloc] initWithUnsignedLong: {code}]";
			case TypeCode.Single:
				return $"[[NSNumber alloc] initWithFloat: {code}]";
			case TypeCode.Double:
				return $"[[NSNumber alloc] initWithDouble: {code}]";
			case TypeCode.String:
			case TypeCode.Object:
				return code;
			default:
				throw new EmbeddinatorException (99, $"Internal error `unexpected type {type} in subscript generation`. Please file a bug report with a test case (https://github.com/mono/Embeddinator-4000/issues");
			}
		}

		internal string FromNSObject (Type type, string code)
		{
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Boolean:
				return $"[{code} boolValue]";
			case TypeCode.SByte:
				return $"[{code} charValue]";
			case TypeCode.Byte:
				return $"[{code} unsignedCharValue]";
			case TypeCode.Int16:
				return $"[{code} shortValue]";
			case TypeCode.Char:
			case TypeCode.UInt16:
				return $"[{code} unsignedShortValue]";
			case TypeCode.Int32:
				return $"[{code} intValue]";
			case TypeCode.UInt32:
				return $"[{code} unsignedIntValue]";
			case TypeCode.Int64:
				return $"[{code} longLongValue]";
			case TypeCode.UInt64:
				return $"[{code} unsignedLongLongValue]";
			case TypeCode.Single:
				return $"[{code} floatValue]";
			case TypeCode.Double:
				return $"[{code} doubleValue]";
			case TypeCode.String:
			case TypeCode.Object:
				return code;
			default:
				throw new EmbeddinatorException (99, $"Internal error `unexpected type {type} in subscript generation`. Please file a bug report with a test case (https://github.com/mono/Embeddinator-4000/issues");
			}
		}
	}
}
