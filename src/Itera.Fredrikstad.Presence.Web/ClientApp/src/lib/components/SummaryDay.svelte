<script lang="ts">
  import { format, isFirstDayOfMonth } from "date-fns";
  import { nb } from "date-fns/locale";

  import { useQuery } from "@sveltestack/svelte-query";
  import { getDayAtWorkItems } from "../api";
  import { users } from "../users";
  import Attendee from "./Attendee.svelte";
  import Avatar from "./Avatar.svelte";
  import type { DayAtWork } from "../models";
  import { emphasizeEmojis } from "../utils";

  export let day: Date;

  $: query =
    day &&
    useQuery(["daySummary", day], () => getDayAtWorkItems(day), {
      staleTime: 1000 * 60 * 5, // 5 minutes
    });

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

  function hasComment(dayAtWork: DayAtWork): boolean {
    return !!dayAtWork.comment;
  }
</script>

<div class="day" class:loading={$query.isLoading}>
  <h1>
    {format(day, "d", { locale: nb })}
    {#if isFirstDayOfMonth(day)}
      {format(day, "MMMM", { locale: nb })}
    {/if}
  </h1>
  <h2>{format(day, "eeee", { locale: nb })}</h2>
  {#each $query?.data?.filter((attendee) => !isEmpty(attendee) || hasComment(attendee)) ?? [] as attendee}
    <div
      class:selected={isSelected(attendee) && !isEmpty(attendee)}
      class:first-half={isFirstHalf(attendee)}
      class:last-half={isLastHalf(attendee)}
      class="attendee"
    >
      <p class="name">{users[attendee.userId]}</p>
      {#if attendee.comment}
        <p class="comment">{@html emphasizeEmojis(attendee.comment)}</p>
      {/if}
    </div>
  {/each}
  <!-- <div class="attendees"> -->
  <!-- {#each $query?.data ?? [] as attendee} -->
  <!-- <Attendee {attendee} /> -->
  <!-- <Avatar {attendee} /> -->
  <!-- {/each} -->
  <!-- </div> -->
</div>

<style>
  .day {
    background-color: #efefef;
    padding: 1rem;
    border: 1px solid #999;
    display: flex;
    flex-direction: column;
    align-content: flex-start;
    justify-content: flex-start;
    align-items: flex-start;
    flex-basis: 20%;
    max-width: 20%;
    flex-shrink: 0;
  }

  :global(.dark) .day {
    background-color: #333;
    border: 1px solid #444;
  }

  @media only screen and (max-width: 480px) {
    .day {
      flex-basis: 100%;
      max-width: 100%;
      margin-bottom: 1rem;
    }
  }

  .day:hover {
    background-color: #eaeaea;
  }

  :global(.dark) .day:hover {
    background-color: #444;
  }

  .day h1 {
    margin: 0;
    font-size: 1rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    max-width: 100%;
  }

  :global(.dark) .day h1,
  :global(.dark) .day h2 {
    background-color: #666;
  }

  .day h2 {
    margin: 0;
    font-size: 1.2rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    max-width: 100%;
  }

  .day p {
    display: block;
    padding: 0.2rem 0.5rem;
    margin: 0.5rem 0;
    max-width: 100%;
    text-overflow: ellipsis;
    overflow: hidden;
  }

  .comment {
    font-size: 0.8rem;
    font-weight: 300;
    white-space: pre-wrap;
  }

  .comment > :global(.emoji) {
    font-size: 1.2rem;
  }

  .attendee {
    margin: 0.5rem 0;
    width: 100%;
    border: 1px solid #ffcccb;
  }

  :global(.dark) .attendee {
    border: 1px solid #ff4b33;
  }

  :global(.dark) .attendee p {
    color: #fff;
  }

  .attendee.selected {
    background: repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  :global(.dark) .attendee.selected {
    background: repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
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

  :global(.dark) .attendee.selected.first-half {
    background: linear-gradient(
        to left,
        #666 0%,
        #666 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
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

  :global(.dark) .attendee.selected.last-half {
    background: linear-gradient(
        to right,
        #666 0%,
        #666 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
  }
</style>
