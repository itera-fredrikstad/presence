<script lang="ts">
  import {
    format,
    eachDayOfInterval,
    startOfISOWeek,
    endOfISOWeek,
    startOfMonth,
    endOfMonth,
    addMonths,
    isSameDay,
    getWeeksInMonth,
    getWeekOfMonth,
    addWeeks,
    addDays,
    weeksToDays,
    getISOWeek,
  } from "date-fns";
  import { nb } from "date-fns/locale";

  import type {
    DayAtWorkItemsMap,
    PublicHolidayMap,
    TeamEventMap,
  } from "../api";
  import type { DayAtWork, Identifiable } from "../models";
  import { getDayId } from "../utils";

  import Day from "./Day.svelte";

  export let today: Date;
  export let dayAtWorks: DayAtWorkItemsMap;
  export let publicHolidays: PublicHolidayMap;
  export let teamEvents: TeamEventMap;
  export let onUpdate: (
    day: Date,
    dayAtWork: Identifiable<DayAtWork>,
    updatedDayAtWork: Partial<DayAtWork>
  ) => void;

  let offset = 0;

  let currentMonthStart = startOfMonth(today);
  let currentMonthEnd = endOfMonth(today);

  $: windows = [...Array(offset + 2).keys()].map((o) =>
    eachDayOfInterval({
      start: addDays(today, o * 5),
      end: addDays(today, (o + 1) * 5 - 1),
    })
  );

  $: currentWindow = windows[offset];
  $: weeks = currentWindow.reduce(
    (p, n) => ({ ...p, [getISOWeek(n)]: true }),
    {}
  );
  $: months = currentWindow.reduce(
    (p, n) => ({ ...p, [format(n, "MMMM", { locale: nb })]: true }),
    {}
  );

  function handleNext() {
    offset++;
  }

  function handlePrev() {
    if (offset > 0) {
      offset--;
    }
  }
</script>

<div class="selector-wrapper">
  <h2>
    Uke {Object.keys(weeks).join("/")} ({Object.keys(months).join("/")})
  </h2>
</div>
<div class="wrapper">
  {#if offset > 0}
    <i class="fa-solid fa-circle-arrow-left" on:click={handlePrev} />
  {/if}
  <i class="fa-solid fa-circle-arrow-right" on:click={handleNext} />
  <div class="slide-container">
    <div
      class="slider slide"
      style="transform: translateX({offset * -100}%); transform-origin: 0 0"
    >
      {#each windows as window}
        <div class="window-container">
          <div class="days">
            {#each window as day}
              {@const dayId = getDayId(day)}
              {@const dayAtWork = dayAtWorks?.[dayId]}
              {@const publicHoliday = publicHolidays?.[dayId]}
              {@const events = teamEvents?.[dayId] ?? []}
              <div class="day-wrapper">
                <Day
                  {day}
                  {dayAtWork}
                  {publicHoliday}
                  teamEvents={events}
                  showDayName={true}
                  isActive={day > today || isSameDay(day, today)}
                  onUpdate={(updated) => onUpdate(day, dayAtWork, updated)}
                />
              </div>
            {/each}
          </div>
        </div>
      {/each}
    </div>
  </div>
</div>

<style>
  .selector-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  h1 {
    font-size: 2rem;
  }

  h2 {
    font-size: 1.5rem;
  }

  .wrapper {
    position: relative;
    width: 100%;
  }

  .slide-container {
    width: 100%;
    overflow: hidden;
  }

  .slider {
    display: flex;
  }

  .window-container {
    overflow: hidden;
    width: 100%;
    flex-shrink: 0;
  }

  @media only screen and (max-width: 480px) {
    .days {
      flex-direction: column;
    }
  }

  .days {
    display: flex;
  }

  .day-wrapper {
    flex-basis: calc(100% / 5);
    flex-shrink: 0;
    flex-direction: column;
  }

  .slide {
    transition: all 0.5s;
  }

  .fa-solid {
    color: #999;
  }

  .fa-solid:hover {
    color: #333;
  }

  .fa-circle-arrow-right {
    position: absolute;
    right: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }

  .fa-circle-arrow-left {
    position: absolute;
    left: -100px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 2rem;
  }
</style>
