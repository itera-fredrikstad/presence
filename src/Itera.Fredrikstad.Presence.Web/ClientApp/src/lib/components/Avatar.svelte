<script lang="ts">
  import type { DayAtWork } from "../models";
  import { users } from "../users";
  import { getInitials } from "../utils";

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
  class="avatar"
  title={attendee.userId}
  class:selected={isSelected(attendee) && !isEmpty(attendee)}
  class:first-half={isFirstHalf(attendee)}
  class:last-half={isLastHalf(attendee)}
>
  <div class="initials">
    {getInitials(attendee.userId)}
  </div>
</div>

<style>
  .avatar {
    display: flex;
    font-size: 1.5rem;
    background-color: #ff4b33;
    width: 4rem;
    height: 4rem;
    border-radius: 2rem;
    justify-content: center;
    align-items: center;
    border: 1px solid #af1dff;
    color: #af1dff;
  }
  .avatar.selected {
    background: repeating-linear-gradient(0, #37e17b, #37e17b 5px, #37e17b 5px, #37e17b 10px);
  }

  .avatar.selected.first-half {
    background: linear-gradient(
        to left,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(0, #37e17b, #37e17b 5px, #37e17b 5px, #37e17b 10px);
  }

  .avatar.selected.last-half {
    background: linear-gradient(
        to right,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(0, #37e17b, #37e17b 5px, #37e17b 5px, #37e17b 10px);
  }
</style>
