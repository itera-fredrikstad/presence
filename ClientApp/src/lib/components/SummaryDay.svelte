<script lang="ts">
  import { format, isFirstDayOfMonth } from "date-fns";
  import { nb } from "date-fns/locale";

  import { useQuery } from "@sveltestack/svelte-query";
  import { getDayAtWorkItems } from "../api";
  import { users } from "../users";
  import Attendee from "./Attendee.svelte";
  import Avatar from "./Avatar.svelte";

  export let day: Date;

  $: query = day && useQuery(["daySummary", day], () => getDayAtWorkItems(day));
</script>

<div class="day" class:loading={$query.isLoading}>
  <h1>
    {format(day, "d", { locale: nb })}
    {#if isFirstDayOfMonth(day)}
      {format(day, "MMMM", { locale: nb })}
    {/if}
  </h1>
  <h2>{format(day, "eeee", { locale: nb })}</h2>
  <!-- {#each $query?.data ?? [] as attendee}
    <div
      class:selected={isSelected(attendee) && !isEmpty(attendee)}
      class:first-half={isFirstHalf(attendee)}
      class:last-half={isLastHalf(attendee)}
      class="attendee"
    >
      <p>{users[attendee.userId]}</p>
    </div>
  {/each} -->
  <div class="attendees">
    {#each $query?.data ?? [] as attendee}
      <!-- <Attendee {attendee} /> -->
      <Avatar {attendee} />
    {/each}
  </div>
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

  .day:hover {
    background-color: #eaeaea;
  }

  .day h1 {
    margin: 0;
    font-size: 1rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    max-width: 100%;
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
  .attendees {
    display: inline-flex;
    flex-wrap: wrap;
    gap: 12px;
  }
</style>
