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
		/// <summary>
		/// Reads a text file with one mapping per line:
		///   [VariableName]|[QuestionName]
		/// 
		/// </summary>
		/// <remarks>
		/// See below for a method that automatically maps variables and questions based on matching names, 
		/// without the need for an extra file to specify the mapping.
		/// </remarks>
		/// <param name="filePath"></param>
		/// <param name="variableSchemeId"></param>
		/// <param name="questionSchemeId"></param>
		/// <param name="onlyTest"></param>
		static void MapVariablesToQuestions(string filePath, IdentifierTriple variableSchemeId, IdentifierTriple questionSchemeId, bool onlyTest)
		{
			var client = RepositoryIntro.GetClient();

			VariableScheme vs = client.GetItem(variableSchemeId, ChildReferenceProcessing.Populate) as VariableScheme;
			QuestionScheme qs = client.GetItem(questionSchemeId, ChildReferenceProcessing.Populate) as QuestionScheme;

			// Read each line of the mapping file.
			int matches = 0;
			foreach (string line in File.ReadLines(filePath))
			{
				// Grab the variable name and question name.
				string[] parts = line.Split(new char[] { '|' });
				if (parts.Length != 2)
				{
					Console.WriteLine("Invalid line: " + line);
					continue;
				}

				string variableName = parts[0];
				string questionName = parts[1];

				// Grab the corresponding variable and question objects. 
				Variable variable = vs.Variables.SingleOrDefault(v => v.ItemName.Current == variableName);
				Question question = qs.Questions.SingleOrDefault(q => q.ItemName.Current == questionName);
				if (variable != null && question != null)
				{
					// Add the question as SourceQuestion of the variable.
					variable.SourceQuestions.Add(question);
					variable.Version++;

					Console.WriteLine(string.Format("Assigning {0} to {1}", question.ItemName.Current, variable.ItemName.Current));

					if (!onlyTest)
					{
						client.RegisterItem(variable, new CommitOptions());
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
				client.RegisterItem(vs, new CommitOptions());
			}

			Console.WriteLine("Done. Found " + matches.ToString() + " matches.");
		}

		/// <summary>
		/// Maps Variables to Questions where the name of the Variable matches the name of 
		/// the Question.
		/// </summary>
		/// <param name="variableSchemeId"></param>
		/// <param name="questionSchemeId"></param>
		/// <param name="onlyTest"></param>
		static void MapVariablesToQuestionsAuto(IdentifierTriple variableSchemeId, IdentifierTriple questionSchemeId, bool onlyTest)
		{
			var client = RepositoryIntro.GetClient();

			CommitOptions commitOptions = new CommitOptions();

			VariableScheme vs = client.GetItem(variableSchemeId, ChildReferenceProcessing.Populate) as VariableScheme;
			QuestionScheme qs = client.GetItem(questionSchemeId, ChildReferenceProcessing.Populate) as QuestionScheme;

			// Grab all variable names and question names.
			var variableNameList = vs.Variables.Select(v => v.ItemName.Current.Substring(2)).ToList();
			var questionNameList = qs.Questions.Select(q => q.ItemName.Current).ToList();

			int matches = 0;
			foreach (string varName in variableNameList)
			{
				string foundQuestionName = questionNameList.Where(q => string.Compare(q, varName, true) == 0).FirstOrDefault();
				if (foundQuestionName != null)
				{
					// If there is a question with the same name as this variable, 
					// grab the Question and Variable objects, and assign the question
					// as a SourceQuestion.
					Variable variable = vs.Variables.SingleOrDefault(v => v.ItemName.Current == varName);
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
