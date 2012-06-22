using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Model;
using ColecticaSdkSamples.Basic;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Ddi;
using System.IO;

namespace ColecticaSdkSamples.Tools
{
	public class VariableToQuestionMapper
	{
		static void MapVariablesToQuestions(bool onlyTest)
		{
			MultilingualString.CurrentCulture = "en-GB";
			VersionableBase.DefaultAgencyId = "cls";

			string filePath = @"D:\Downloads\filesforMinneapolis\bcs2008_data_question_map.txt";

			var client = RepositoryIntro.GetClient();

			CommitOptions commitOptions = new CommitOptions();

			VariableScheme vs = client.GetItem(new Guid("3f56bd8c-3a1e-43cb-86cb-da3343b6b7ee"), "ucl.ac.uk", 1, ChildReferenceProcessing.Populate) as VariableScheme;
			QuestionScheme qs = client.GetItem(new Guid("906a03ff-d59b-4ac7-b9a9-2b3f455b4be6"), "cls", 2, ChildReferenceProcessing.Populate) as QuestionScheme;

			int matches = 0;
			foreach (string line in File.ReadLines(filePath))
			{
				string[] parts = line.Split(new char[] { '|' });
				if (parts.Length != 2)
				{
					Console.WriteLine("Invalid line: " + line);
					continue;
				}

				string variableName = parts[0];
				string questionName = parts[1];

				Variable variable = vs.Variables.SingleOrDefault(v => v.ItemName.Current == variableName);
				Question question = qs.Questions.SingleOrDefault(q => q.ItemName.Current == questionName);

				if (variable != null && question != null)
				{
					variable.SourceQuestions.Add(question);
					variable.Version++;
					Console.WriteLine(string.Format("Assigning {0} to {1}", question.ItemName.Current, variable.ItemName.Current));

					if (!onlyTest)
					{
						client.RegisterItem(variable, commitOptions);
					}

					matches++;
				}
				else
				{
					Console.WriteLine(string.Format("No match for {0} or {1}", variableName, questionName));
				}
			}

			vs.Version++;
			if (!onlyTest)
			{
				client.RegisterItem(vs, commitOptions);
			}

			Console.WriteLine("Done. Found " + matches.ToString() + " matches.");
		}

		static void MapVariablesToQuestionsAuto(bool onlyTest)
		{
			MultilingualString.CurrentCulture = "en-GB";
			VersionableBase.DefaultAgencyId = "cls";

			string filePath = @"d:\Downloads\Mapping.txt";

			var client = RepositoryIntro.GetClient();

			CommitOptions commitOptions = new CommitOptions();

			VariableScheme vs = client.GetItem(new Guid("3f56bd8c-3a1e-43cb-86cb-da3343b6b7ee"), "ucl.ac.uk", 1, ChildReferenceProcessing.Populate) as VariableScheme;
			QuestionScheme qs = client.GetItem(new Guid("906a03ff-d59b-4ac7-b9a9-2b3f455b4be6"), "cls", 2, ChildReferenceProcessing.Populate) as QuestionScheme;

			var variableNameList = vs.Variables.Select(v => v.ItemName.Current.Substring(2)).ToList();
			var questionNameList = qs.Questions.Select(q => q.ItemName.Current).ToList();

			int matches = 0;
			//foreach (string line in File.ReadLines(filePath))
			foreach (string varName in variableNameList)
			{
				string foundQuestionName = questionNameList.Where(q => string.Compare(q, varName, true) == 0).FirstOrDefault();
				if (foundQuestionName != null)
				{
					Variable variable = vs.Variables.SingleOrDefault(v => v.ItemName.Current == "b8" + varName);
					Question question = qs.Questions.SingleOrDefault(q => q.ItemName.Current == foundQuestionName);

					if (variable != null && question != null)
					{
						variable.SourceQuestions.Add(question);
						variable.Version++;
						Console.WriteLine(string.Format("Assigning {0} to {1}", question.ItemName.Current, variable.ItemName.Current));

						if (!onlyTest)
						{
							client.RegisterItem(variable, commitOptions);
						}

						matches++;
					}
					else
					{
						Console.WriteLine(string.Format("No match for {0} or {1}", varName, foundQuestionName));
					}
				}
			}

			vs.Version++;
			if (!onlyTest)
			{
				client.RegisterItem(vs, commitOptions);
			}

			Console.WriteLine("Done. Found " + matches.ToString() + " matches.");
		}
	}
}
