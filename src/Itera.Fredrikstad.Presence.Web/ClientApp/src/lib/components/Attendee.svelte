<script lang="ts">
  import type { DayAtWork } from "../models";
  import { users } from "../users";

  export let attendee: DayAtWork;

  function isSelected(dayAtWork: DayAtWork): boolean {
    return !!dayAtWork && !!dayAtWork.type;
  }

  function isFull(dayAtWork: DayAtWork): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "FULL";
  }

  function isFirstHalf(dayAtWork: DayAtWork): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "FIRST-HALF";
  }

  function isLastHalf(dayAtWork: DayAtWork): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "LAST-HALF";
  }

  function isEmpty(dayAtWork: DayAtWork): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "EMPTY";
  }
</script>

<div
  class:selected={isSelected(attendee) && !isEmpty(attendee)}
  class:first-half={isFirstHalf(attendee)}
  class:last-half={isLastHalf(attendee)}
  class="attendee"
>
  <p>{users[attendee.userId]}</p>
</div>

<style>
  .attendee {
    margin: 0.5rem 0;
    width: 100%;
  }

  .attendee.selected {
    background: repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  .attendee.selected.first-half {
    background: linear-gradient(
        to left,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  .attendee.selected.last-half {
    background: linear-gradient(
        to right,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  .attendee p {
    padding: 0.2rem 0.5rem;
    margin: 0.5rem 0;
  }
</style>
