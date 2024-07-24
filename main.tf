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

resource "aws_lambda_function" "dayhandler_lambda_func" {
filename                       = "DayHandler/src/DayHandler/lambda.zip"
function_name                  = "DayHandler_Function"
role                           = aws_iam_role.iam_for_lambda.arn
handler                        = "DayHandler::DayHandler.Function::FunctionHandler"
runtime                        = "dotnet8"
timeout			       = 60
}

resource "aws_lambda_function" "getteamratings_lambda_func" {
filename                       = "GetTeamRatings/src/GetTeamRatings/lambda.zip"
function_name                  = "GetTeamRatings_Function"
role                           = aws_iam_role.iam_for_lambda.arn
handler                        = "GetTeamRatings::GetTeamRatings.Function::FunctionHandler"
runtime                        = "dotnet8"
timeout			       = 60
}

resource "aws_lambda_function_url" "getteamratings_lambda_url" {
  function_name      = aws_lambda_function.getteamratings_lambda_func.function_name
  authorization_type = "NONE"

  cors {
    allow_credentials = true
    allow_origins     = ["*"]
    allow_methods     = ["*"]
    allow_headers     = ["date", "keep-alive"]
    expose_headers    = ["keep-alive", "date"]
    max_age           = 86400
  }
}

resource "aws_cloudwatch_event_rule" "dayhandler_rule" {
    name = "update-team-rankings-daily"
    description = "Runs DayHandler Lambda function each day at 10am UTC / 4am MST"
    schedule_expression = "cron(0 10 * * ? *)"
}

resource "aws_cloudwatch_event_target" "dayhandler_target" {
    arn = aws_lambda_function.dayhandler_lambda_func.arn
    rule = aws_cloudwatch_event_rule.dayhandler_rule.id
}

resource "aws_lambda_permission" "allow_cloudwatch" {
    statement_id  = "AllowExecutionFromCloudWatch"
    action        = "lambda:InvokeFunction"
    function_name = aws_lambda_function.dayhandler_lambda_func.function_name
    source_arn = aws_cloudwatch_event_rule.dayhandler_rule.arn
    principal = "events.amazonaws.com"
    
}