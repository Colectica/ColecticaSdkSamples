# This is a simple Ruby program to demonstrate
# the use of Colectica SDK within IronRuby.

# First, load the Colectica SDK assemblies.
require 'log4net.dll'
require 'Algenta.Colectica.Model.dll'
require 'Algenta.Colectica.Model.Ddi.dll'

# Include some useful namespaces.
include Algenta::Colectica::Model
include Algenta::Colectica::Model::Ddi

# Create a new question and set the question text.
question = Question.new
question.question_text["en-US"] = "What is your name?"

# Output the question text to the screen.
puts "Question Text: " + question.question_text.current

# Prompt for some input so the screen doesn't disappear 
# if it was run from the debugger.
puts 'Enter some text to exit'
test = gets.chomp

