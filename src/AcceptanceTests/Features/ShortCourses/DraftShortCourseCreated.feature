Feature: DraftShortCourseCreated

Scenario: Create Draft Short Course
	Given SLD has informed the system that a new short course has been created
	Then a short course record is created

Scenario: Create Draft Short Course for a learner that already exists (in apprenticeships)
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice | Uln   |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      | 54321 |
	And SLD call the create short course endpoint with the following information
		| FirstName | LastName | Uln   |
		| Bob       | Newname  | 54321 |
	Then a short course record is created with
		| FirstName | LastName |
		| Bob       | Newname  |
	And the Apprenticeship Learners name is updated to Bob Newname

Scenario: Create Draft Short Course with learning support
    Given SLD call the create short course endpoint with the following information
		| FirstName | LastName | Uln   | LearningSupport[0]                             |
		| Bob       | Newname  | 54321 | startDate:currentAY-09-25 endDate:nextAY-07-31 |
	Then a short course record is created with
		| LearningSupport[0]                             |
		| startDate:currentAY-09-25 endDate:nextAY-07-31 |

Scenario: Approve Short Course
	Given SLD has informed the system that a new short course has been created
	When the Short Course has been approved by an employer
	Then the Short Course is approved

Scenario: Create Draft is called for a learner with an approved short course (Nothing should happen)
	Given SLD call the create short course endpoint with the following information
		| Uln   |
		| 54321 |
	And short course is approved
	And SLD call the create short course endpoint with the following information
		| Uln   |
		| 54321 |
	Then the response from the create short course endpoint is 409
	And for learner with Uln 54321 there is 1 short course record

Scenario: Create Draft is called for a learner with an unapproved short course (unapproved record is updated)
	Given SLD call the create short course endpoint with the following information
		| Uln   | FirstName | LastName | Milestone                     | LearningSupport[0]                             |
		| 54321 | Robert    | Oldname  | ThirtyPercentLearningComplete | startDate:currentAY-09-25 endDate:nextAY-07-31 |
	And SLD call the create short course endpoint with the following information
		| Uln   | FirstName | LastName | Milestone        | LearningSupport[0]                             |
		| 54321 | Bob       | Newname  | LearningComplete | startDate:currentAY-08-25 endDate:nextAY-06-30 |
	Then the response from the create short course endpoint is 200
	And for learner with Uln 54321 there is 1 short course record
	And a short course record exists with
		| FirstName | LastName | LearningSupport[0]                             | Milestone        |
		| Bob       | Newname  | startDate:currentAY-08-25 endDate:nextAY-06-30 | LearningComplete |
