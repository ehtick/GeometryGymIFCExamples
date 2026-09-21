using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

using GeometryGym.Ifc;

namespace CustomImportIFC
{
	public class CustomImportIFC : Command
	{
		public CustomImportIFC()
		{
			Instance = this;
		}
		public static CustomImportIFC Instance { get; private set; }
		public override string EnglishName => "CustomImportIFC";
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			return ImportIFC();
		}
		private Result ImportIFC()
		{
			var doc = RhinoDoc.ActiveDoc;
			var ifcPath = UtilityIFC.SeekOpenPath();
			if (string.IsNullOrEmpty(ifcPath))
				return Result.Cancel;

			DatabaseIfc db = new DatabaseIfc(ifcPath);
			IfcRepresentationItem.GeometryGenerateOptions geometryOptions = new IfcRepresentationItem.GeometryGenerateOptions(db);
			geometryOptions.DeviationTolerance = 0.2;
			geometryOptions.ProfileFillets = false;
			geometryOptions.IgnoreUnsuccessfulBooleanDifference = true;

			IfcObjectDefinition.RhinoGenerateOptions options = new IfcObjectDefinition.RhinoGenerateOptions(geometryOptions);
			//options.IncludedFilter = new MyEnhancedFilter();
			options.UserTextFromProperties = true;
			options.PrefixUserTextPropertySetName = true;
			options.LocalCoordinatesProjectedOrSite = false;
			options.LayerMode = IfcObjectDefinition.RhinoGenerateOptions.LayerGeneration.Assigned;
			options.SaveGlobalIdToLayer = false;
			options.MaterialsFromPresentationStyle = false;
			options.AssemblyCreationMode = IfcObjectDefinition.RhinoGenerateOptions.AssemblyMode.Default;
			options.GroupAggregatedElements = false;

			if (db.ImportModel(doc, options))
				return Result.Success;
			return Result.Failure;
		}
	}
	public class MyEnhancedFilter : ApplicableFilter
	{
		public MyEnhancedFilter() : base("IfcWall,IfcCurtainWall") { }
		public override bool IsApplicable(IfcObjectDefinition obj)
		{
			string typeName = "MyCustomType";
			if(obj is IfcObject o)
			{
				if (string.Compare(o.ObjectType, "MyCustomType", true) == 0)
					return base.IsApplicable(obj);
				var typeObject = o.RelatingType();
				if (typeObject != null && string.Compare(typeObject.Name, typeName, true) == 0)
					return base.IsApplicable(obj);
			}
			return false;
		}
	}
}
