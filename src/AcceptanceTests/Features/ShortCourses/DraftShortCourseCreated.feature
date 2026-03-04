Feature: DraftShortCourseCreated

Scenario: Create Draft Short Course
	Given SLD has informed the system that a new short course has been created
	Then a short course record is created

Scenario: Create Draft Short Course for a learner that already exists (in apprenticeships)
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice | Uln   |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      | 54321 |
	And A new SLD course with the following information is sent
		| FirstName | LastName | Uln   |
		| Bob       | Newname  | 54321 |
	Then a short course record is created with
		| FirstName | LastName |
		| Bob       | Newname  |
	And the Apprenticeship Learners name is updated to Bob Newname

Scenario: Create Draft Short Course with learning support
    Given A new SLD course with the following information is sent
		| FirstName | LastName | Uln   | LearningSupport[0]                             |
		| Bob       | Newname  | 54321 | startDate:currentAY-09-25 endDate:nextAY-07-31 |
	Then a short course record is created with
		| LearningSupport[0]                             |
		| startDate:currentAY-09-25 endDate:nextAY-07-31 |

Scenario: Approve Short Course
	Given SLD has informed the system that a new short course has been created
	When the Short Course has been approved by an employer
	Then the Short Course is approved
