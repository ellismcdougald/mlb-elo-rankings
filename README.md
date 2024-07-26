# Description

This is a website that tracks and displays Elo ratings for Major League Baseball teams for the 2024 regular season. The Elo rating system measures the relative strength of teams. Each team is given a starting Elo rating of 1500. A team's rating is adjusted after each game they play, depending on the outcome and the strength of their opponent. If a team with a low ELO rating beats a team with a high ELO rating, the ELO rating of that winning team will increase by more than if they beat another low-rated team. Conversely, if a top team loses to a weak team, the top team will see a larger decrease in the rating than if they had lost to another top team.

# Images

## Ratings

![Screenshot of the Ratings page](/ratings-screenshot.png?raw=true)

## How It Works

![Screenshot of the How It Works page](/how-it-works-screenshot.png?raw=true)

# How Is It Built?

The frontend is built using AngularJS. The backend uses the .NET framework and is written in C#. It uses AWS Lambda functions to maintain and interact with an AWS DynamoDB database. I used Terraform to provision these AWS resources.

I undertook this project because I wanted to gain some experience with several technologies that were new to me. This was my first exposure to AngularJS, C# and .NET, AWS, and Terraform.

# Contact

Email me: ecmcdougald@gmail.com
