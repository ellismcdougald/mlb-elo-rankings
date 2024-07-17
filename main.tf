terraform {
  required_providers {
    aws = {
      source: "hashicorp/aws"
    }
  }
}

provider "aws" {
  region = "us-west-2"
  shared_credentials_files = ["$HOME/.aws/credentials"]
}

resource "aws_dynamodb_table" "basic-dynamodb-table" {
  name = "MLBElo-TeamRatings"
  billing_mode = "PROVISIONED"
  read_capacity = 10
  write_capacity = 10
  hash_key = "ID"

  attribute {
    name = "ID"
    type = "N"
  }
}

resource "aws_iam_role" "iam_for_lambda" {
 name = "iam_for_lambda"

 assume_role_policy = jsonencode({
   "Version" : "2012-10-17",
   "Statement" : [
     {
       "Effect" : "Allow",
       "Principal" : {
         "Service" : "lambda.amazonaws.com"
       },
       "Action" : "sts:AssumeRole"
     }
   ]
  })
}
          
resource "aws_iam_role_policy_attachment" "lambda_policy" {
   role = aws_iam_role.iam_for_lambda.name
   policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}
          
resource "aws_iam_role_policy" "dynamodb-lambda-policy" {
   name = "dynamodb_lambda_policy"
   role = aws_iam_role.iam_for_lambda.id
   policy = jsonencode({
      "Version" : "2012-10-17",
      "Statement" : [
        {
           "Effect" : "Allow",
           "Action" : ["dynamodb:*"],
           "Resource" : "${aws_dynamodb_table.basic-dynamodb-table.arn}"
        }
      ]
   })
}

resource "aws_lambda_function" "terraform_basic_lambda_func" {
filename                       = "DayHandler/src/DayHandler/lambda.zip"
function_name                  = "DayHandler_Function"
role                           = aws_iam_role.iam_for_lambda.arn
handler                        = "DayHandler::DayHandler.Function::FunctionHandler"
runtime                        = "dotnet8"
timeout			       = 60
}
