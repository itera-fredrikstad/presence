<script lang="ts">
  import { afterUpdate } from "svelte";
  import { format, isFirstDayOfMonth } from "date-fns";
  import { nb } from "date-fns/locale";

  import type { Identifiable, DayAtWork, DayAtWorkType } from "../models";
  import { useQuery } from "@sveltestack/svelte-query";
  import { getDayAtWorkItems } from "../api";

  export let day: Date;

  $: query = day && useQuery(["daySummary", day], () => getDayAtWorkItems(day));

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

<div class="day">
  <h1>
    {format(day, "d", { locale: nb })}
    {#if isFirstDayOfMonth(day)}
      {format(day, "MMMM", { locale: nb })}
    {/if}
  </h1>
  <h2>{format(day, "eeee", { locale: nb })}</h2>
  {#each $query?.data ?? [] as attendee}
    <div
      class:selected={isSelected(attendee) && !isEmpty(attendee)}
      class:first-half={isFirstHalf(attendee)}
      class:last-half={isLastHalf(attendee)}
      class="attendee"
    >
      <p>{attendee.userId}</p>
    </div>
  {/each}
</div>

<style>
  .day {
    aspect-ratio: 1;
    background-color: #efefef;
    padding: 1rem;
    border: 1px solid #999;
    display: flex;
    flex-direction: column;
    align-content: flex-start;
    justify-content: flex-start;
    align-items: flex-start;
    flex-basis: 20%;
    flex-shrink: 0;
  }

  .day:hover {
    background-color: #eaeaea;
  }

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

  .day h1 {
    margin: 0;
    font-size: 1rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
  }

  .day h2 {
    margin: 0;
    font-size: 1.2rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
  }

  .day p {
    padding: 0.2rem 0.5rem;
    margin: 0.5rem 0;
  }
</style>
