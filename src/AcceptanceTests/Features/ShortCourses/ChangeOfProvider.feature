Feature: Change of Provider
    Lifecycle isolation tests for short course change of provider. Covers FLP-1801 and FLP-1864. Essentially, following a Change of Provider, each Provider
    should be able to continue to update their specific episode without impacting the other. (That is, no impact in Learning. Earnings impact is covered elsewhere).

Background:
    Given a short course exists with Provider A
    And a Change of Provider has occurred, creating an episode with Provider B

Scenario: Change of provider creates a new episode
    Then the response from the create short course endpoint is 200
    And the short course has 2 episodes

Scenario: Update to Provider B does not affect Provider A's episode
    When SLD withdraws Provider B from the short course
    Then Provider B's episode is withdrawn
    And Provider A's episode is not withdrawn

Scenario: Update to Provider A does not affect Provider B's episode
    When SLD calls the update short course endpoint for Provider A with the 30% milestone
    Then Provider A's episode has the milestone recorded
    And Provider B's episode has no milestone

Scenario: Removal of Provider B's episode does not affect Provider A's episode
    When SLD removes the short course for Provider B
    Then Provider B's episode IsRemoved is True
    And Provider A's episode IsRemoved is False

Scenario: Reinstate Provider B's episode does not affect Provider A's episode
    Given SLD has removed the short course for Provider B
    When SLD reinstates the short course for Provider B
    Then Provider B's episode IsRemoved is False
    And Provider A's episode IsRemoved is False

# Test commented out until bug FLP-1868 is fixed.
#Scenario: Provider A updating their episode does not revert a completion recorded by Provider B
#    Given Provider B has completed the short course
#    When SLD calls the update short course endpoint for Provider A
#    Then the short course status is Complete
